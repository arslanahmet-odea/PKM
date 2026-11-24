using System.Collections.Concurrent;
using CausalCampaign.Domain.Entities;
using CausalCampaign.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace CausalCampaign.PosEnrichment.Caching;

/// <summary>
/// Provides sub-millisecond O(1) POS merchant cache management across L1 (in-memory ConcurrentDictionary)
/// and L2 (EF Core CasualCampaign_MERCHANT_MASTER_CACHE).
/// </summary>
public sealed class PosMerchantCacheManager
{
    private readonly ConcurrentDictionary<string, MerchantMasterCache> _l1MemoryCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly int _maxL1Entries;

    public PosMerchantCacheManager(int maxL1Entries = 50_000)
    {
        _maxL1Entries = Math.Max(100, maxL1Entries);
    }

    /// <summary>
    /// Builds a composite L1 cache key combining normalized DE-43 merchant name and DE-18 MCC code
    /// to prevent cross-MCC cache poisoning (e.g., Grocery 5411 vs Quasi-Cash 6051 under the same facilitator).
    /// </summary>
    public static string BuildCompositeCacheKey(string normalizedKey, string trimmedMcc)
        => $"{normalizedKey}|{trimmedMcc}";

    /// <summary>
    /// Normalizes a raw merchant key (uppercase and single spaces, collapsing all whitespace characters).
    /// </summary>
    public static string NormalizeMerchantKey(string rawMerchantName)
    {
        if (string.IsNullOrWhiteSpace(rawMerchantName))
        {
            return string.Empty;
        }

        string[] parts = rawMerchantName.Trim().ToUpperInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts);
    }

    /// <summary>
    /// Looks up a merchant entry in the L1 memory cache first (composite key: Merchant + MCC),
    /// falling back to the L2 EF Core database cache when MCC matches.
    /// </summary>
    public async Task<MerchantMasterCache?> TryGetFromCacheAsync(
        string normalizedKey,
        string trimmedMcc,
        ICausalCampaignDbContext? dbContext,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedKey))
        {
            return null;
        }

        string compositeKey = BuildCompositeCacheKey(normalizedKey, trimmedMcc);
        if (_l1MemoryCache.TryGetValue(compositeKey, out var cachedItem))
        {
            return cachedItem;
        }

        if (dbContext?.MerchantMasterCaches is null)
        {
            return null;
        }

        MerchantMasterCache? dbEntry = dbContext.MerchantMasterCaches.Provider is IAsyncQueryProvider
            ? await dbContext.MerchantMasterCaches
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.RawMerchantKey == normalizedKey && m.RawMcc_DE18 == trimmedMcc, cancellationToken)
            : dbContext.MerchantMasterCaches
                .FirstOrDefault(m => m.RawMerchantKey == normalizedKey && m.RawMcc_DE18 == trimmedMcc);

        if (dbEntry is not null)
        {
            SetL1Bounded(compositeKey, dbEntry);
        }

        return dbEntry;
    }

    /// <summary>
    /// Stores a newly resolved merchant entry in the L1 memory cache and optionally persists it to the L2 EF Core database table.
    /// </summary>
    public async Task StoreInCacheAsync(
        MerchantMasterCache cacheEntry,
        ICausalCampaignDbContext? dbContext,
        bool persistToDatabase,
        CancellationToken cancellationToken = default)
    {
        string compositeKey = BuildCompositeCacheKey(cacheEntry.RawMerchantKey, cacheEntry.RawMcc_DE18);
        SetL1Bounded(compositeKey, cacheEntry);

        if (!persistToDatabase || dbContext?.MerchantMasterCaches is null)
        {
            return;
        }

        bool existsInDb = dbContext.MerchantMasterCaches.Provider is IAsyncQueryProvider
            ? await dbContext.MerchantMasterCaches
                .AsNoTracking()
                .AnyAsync(m => m.RawMerchantKey == cacheEntry.RawMerchantKey, cancellationToken)
            : dbContext.MerchantMasterCaches
                .Any(m => m.RawMerchantKey == cacheEntry.RawMerchantKey);

        if (!existsInDb)
        {
            try
            {
                dbContext.AddEntity(cacheEntry);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Concurrent insert race on unique RawMerchantKey is benign; entry is already cached in L1.
            }
        }
    }

    private void SetL1Bounded(string compositeKey, MerchantMasterCache entry)
    {
        if (_l1MemoryCache.Count >= _maxL1Entries && !_l1MemoryCache.ContainsKey(compositeKey))
        {
            // Evict a small batch to keep memory bounded under noisy DE-43 traffic
            int toEvict = Math.Max(1, _maxL1Entries / 10);
            int evicted = 0;
            foreach (string key in _l1MemoryCache.Keys)
            {
                if (_l1MemoryCache.TryRemove(key, out _))
                {
                    evicted++;
                    if (evicted >= toEvict)
                    {
                        break;
                    }
                }
            }
        }

        _l1MemoryCache[compositeKey] = entry;
    }

    /// <summary>Gets the number of entries currently in the L1 memory cache.</summary>
    public int L1CacheCount => _l1MemoryCache.Count;
}
