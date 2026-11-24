# RFC-001: Deepening `CausalCampaign` Architecture via Ports & Adapters Ergonomic Hybrid

**Status:** Proposed  
**Architecture Style:** Ports & Adapters (Hexagonal) + Caller-Ergonomic Deep Modules  
**Scope:** All 5 Deepening Clusters (`Optimization & Uplift`, `Spend Tracking & Reward Ledger`, `POS Enrichment & L1/L2 Cache`, `Persistence & Unit of Work`, `Executive 8-KPI Analytics`)

---

## Problem

Across the `CausalCampaign` codebase, core business concepts are fragmented across shallow wrappers and split across architectural boundaries, creating maintenance friction and subtle integration bugs:

1. **Temporal & Conceptual Split in Optimization (`Cluster 1`):**
   - Causal uplift estimation generates heuristic preview targets/rewards and XAI explanations *before* the MILP solver selects the actual micro-bucket, causing persisted XAI rationales to diverge from the customer's assigned offer.
   - Alternative offer selection is disconnected from `ThompsonSamplingBanditService` posterior feedback (`RecordCustomerChoiceFeedback` is never called in production flows), and fallback feature construction duplicates feature-store logic.
2. **Shared Mutable State Duplication (`Cluster 2`):**
   - Real-time card authorization, reversal/refund clawback, and alternative-offer target switching independently mutate `OfferProgressTracker`, `CampaignTemplate.ConsumedBudget`, and `RewardFulfillmentQueue`.
   - Offer switching bypasses `VerifyOperationalStatus`, and optimization persists assignments and trackers in two separate `SaveChangesAsync` calls (partial-commit risk).
3. **Cache / Unit-of-Work Seam Leakage (`Cluster 3`):**
   - POS enrichment passes `ICausalCampaignDbContext?` into the singleton cache manager, which invokes `SaveChangesAsync()` mid-flight during authorization—prematurely flushing unrelated pending changes on the caller's `DbContext`.
   - The 3-layer classification interface is shallow and bypassed by direct static/concrete calls between layers.
4. **Leaky Persistence & Query Workarounds (`Cluster 4`):**
   - `ICausalCampaignDbContext` exposes 9 raw `IQueryable<T>` sets, forcing Application, POS, and CausalAi projects to reference `Microsoft.EntityFrameworkCore` and duplicate `Provider is IAsyncQueryProvider` async/sync branching solely for `Moq` tests, while pre-compiled EF queries sit unused and diverge from service logic.
5. **Hardcoded Executive KPIs & Full-Table Scans (`Cluster 5`):**
   - The executive dashboard returns hardcoded constants for 7 of 8 KPIs while loading the entire `TransactionLedgerFeeds` table into memory to average latency across all campaigns, leaving `ICupedExperimentEvaluator` and `IDriftAndClusteringService` disconnected from management reporting.

---

## Proposed Interface

The **Ports & Adapters Ergonomic Hybrid** consolidates the 5 clusters into 5 deep modules with narrow, caller-ergonomic public facades and clean outbound persistence/telemetry ports.

### 1. Portfolio Optimization & Preference Learning (`ICampaignPortfolioEngine`)

```csharp
public enum OptimizationExecutionMode
{
    Commit,
    DryRun // Simulates R-Learner + MILP + XAI without persisting assignments or trackers
}

public interface ICampaignPortfolioEngine
{
    Task<Result<PortfolioOptimizationResultDto>> OptimizeAndAssignAsync(
        int campaignId,
        OptimizationExecutionMode mode = OptimizationExecutionMode.Commit,
        CancellationToken ct = default);

    Task<Result<AlternativeOfferOption>> SelectOfferPreferenceAsync(
        SelectAlternativeOfferRequestDto request,
        CancellationToken ct = default);

    IReadOnlyList<MicroBucketDefinitionDto> GetMicroBucketCatalog();
}
```

- **Usage:**
  ```csharp
  // Controller / Blazor Back-Office: single call replaces R-Learner -> MILP -> XAI -> Bundle -> Tracker creation
  var result = await _portfolioEngine.OptimizeAndAssignAsync(campaignId, OptimizationExecutionMode.Commit, ct);

  // Mobile Simulator: updates assignment, reconciles tracker/budget via Cluster 2, and feeds Bandit posterior
  var choice = await _portfolioEngine.SelectOfferPreferenceAsync(request, ct);
  ```
- **Complexity Hidden Internally:** Feature-view loading with profile fallback, Kill-Switch validation, R-Learner uplift + propensity scoring, Stratified RCT holdout splitting, Google.OrTools MILP solving, **post-MILP XAI rationale generation** (using the actual assigned bucket target/reward), 3-option equivalent bundle creation, atomic assignment + tracker persistence, and closed-loop Thompson Sampling Bandit posterior updates.

---

### 2. Spend Tracking, Anti-Gaming & Reward Ledger (`ISpendTrackingAndRewardLedger`)

```csharp
public interface ISpendTrackingAndRewardLedger
{
    Task<Result<TrackerUpdateResultDto>> ProcessAuthorizationAsync(
        AuthorizeTransactionRequestDto request,
        CancellationToken ct = default);

    Task<Result<TrackerUpdateResultDto>> ProcessReversalAsync(
        ReversalTransactionRequestDto request,
        CancellationToken ct = default);

    Task<Result> TriggerKillSwitchAsync(
        TriggerKillSwitchRequestDto request,
        CancellationToken ct = default);

    Task<Result<OfferProgressTracker>> ReconcileTargetAdjustmentAsync(
        int customerId,
        int campaignId,
        decimal newTargetAmount,
        decimal newRewardAmount,
        string newRewardType,
        DateTime utcNow,
        CancellationToken ct = default);
}
```

- **Usage:**
  ```csharp
  // ISO 8583 Authorization / Reversal endpoints:
  return ToActionResult(await _rewardLedger.ProcessAuthorizationAsync(request, ct));

  // Called by Cluster 1 when a customer selects a lower-target alternative offer:
  var reconciled = await _rewardLedger.ReconcileTargetAdjustmentAsync(
      customerId, campaignId, option.Target, option.Reward, option.RewardType, DateTime.UtcNow, ct);
  ```
- **Complexity Hidden Internally:** FluentValidation, `AuthorizationCode` / `ReversalAuthCode` idempotency, campaign operational status (`VerifyOperationalStatus`), Anti-Gaming Rule 1 (excluded MCCs) and Rule 2 (symmetric `[T-30m, T+30m]` split-transaction window), cumulative refund validation, `OfferProgressTracker` progression, `CampaignTemplate.AllocateRewardBudget` / `ReleaseRewardBudget`, `RewardFulfillmentQueue` enqueueing/clawback, and single-transaction `IUnitOfWork` commit.

---

### 3. POS Merchant Classification & Async L2 Cache (`IMerchantClassifier` + `IMerchantCacheStorePort`)

```csharp
public readonly record struct MerchantClassificationOptions(
    bool BypassL1Cache = false,
    bool PersistNewL2Entries = true);

public interface IMerchantClassifier
{
    ValueTask<Result<PosEnrichmentResultDto>> ClassifyAsync(
        string rawMerchantNameDe43,
        string rawMccDe18,
        MerchantClassificationOptions options = default,
        CancellationToken ct = default);

    Task<Result<PosEvaluationMetricsDto>> EvaluateMacroF1Async(
        IReadOnlyList<(string RawMerchantDe43, string RawMccDe18, string ExpectedSector)>? labeledSamples = null,
        CancellationToken ct = default);
}

// Outbound Port (owned by Infrastructure in prod, in-memory dictionary in tests)
public interface IMerchantCacheStorePort
{
    Task<MerchantMasterCache?> FindAsync(string normalizedKey, string mcc, CancellationToken ct = default);
    Task UpsertIfAbsentAsync(MerchantMasterCache entry, CancellationToken ct = default);
}
```

- **Usage:**
  ```csharp
  // Inside Cluster 2 hot path (< 1 ms on L1 hit, zero Task allocation, zero SaveChangesAsync side effects):
  var pos = await _merchantClassifier.ClassifyAsync(request.RawMerchantName_DE43, request.RawMcc_DE18, ct: ct);
  ```
- **Complexity Hidden Internally:** Key normalization, composite `(NormalizedMerchant, MCC)` L1 cache with bounded eviction, L2 cache lookup via `IMerchantCacheStorePort`, 3-layer cascade (Brand Dictionary → Payment Facilitator Regex → ML.NET LightGBM), excluded-MCC override, and **deferred write-behind** via an internal `Channel<MerchantMasterCache>` drained by a background worker so L2 writes never pollute the caller's `DbContext` transaction.

---

### 4. Task-Focused Persistence & Unit of Work (`ICampaignLedgerStore` + `IUnitOfWork`)

```csharp
public interface ICampaignLedgerStore
{
    Task<CampaignTemplate?> GetCampaignAsync(int campaignId, CancellationToken ct = default);
    Task<OfferProgressTracker?> GetTrackerAsync(int customerId, int campaignId, CancellationToken ct = default);
    Task<CustomerOfferAssignment?> GetAssignmentAsync(int customerId, int campaignId, CancellationToken ct = default);
    Task<CustomerOfferAssignment?> GetAssignmentByIdAsync(int assignmentId, CancellationToken ct = default);

    Task<bool> ExistsAuthorizationAsync(int customerId, string authCode, bool isReversal, CancellationToken ct = default);
    Task<bool> ExistsSameBrandTxInWindowAsync(int customerId, string brand, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
    Task<TransactionLedgerFeed?> GetTransactionByIdAsync(long transactionId, CancellationToken ct = default);
    Task<decimal> GetPriorRefundedAmountAsync(long originalTransactionId, CancellationToken ct = default);

    Task<IReadOnlyList<RewardFulfillmentQueue>> GetActiveRewardsForTrackerAsync(int trackerId, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerCausalFeatureView>> GetCausalFeaturesOrFallbackAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CustomerOfferAssignment>> GetCampaignAssignmentsAsync(int campaignId, CancellationToken ct = default);
    Task<CustomerCallCenterAuditDto?> GetCustomerAuditSnapshotAsync(int customerId, int campaignId, CancellationToken ct = default);

    void Add<TEntity>(TEntity entity) where TEntity : class;
    void Update<TEntity>(TEntity entity) where TEntity : class;
}

public interface IUnitOfWork
{
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

- **Usage:**
  ```csharp
  await _unitOfWork.ExecuteInTransactionAsync(async token =>
  {
      var tracker = await _store.GetTrackerAsync(request.CustomerId, request.CampaignId, token);
      bool isSplit = await _store.ExistsSameBrandTxInWindowAsync(
          request.CustomerId, pos.CleanedBrandName, windowStart, windowEnd, token);
      // ... domain state mutations ...
      await _unitOfWork.SaveChangesAsync(token);
  }, ct);
  ```
- **Complexity Hidden Internally:** EF Core `EF.CompileAsyncQuery` execution, `AsNoTracking()` read projections, SQL Server `EnableRetryOnFailure` execution-strategy transaction scoping, optimistic concurrency (`RowVersion`), and complete elimination of `IQueryable<T>` / `IAsyncQueryProvider` branching outside `Infrastructure`.

---

### 5. Live 8-KPI Executive Analytics (`ICampaignAnalyticsService` + `ICampaignTelemetryReadPort`)

```csharp
public enum KpiEvaluationMode
{
    LiveComputed,
    FastCached
}

public interface ICampaignAnalyticsService
{
    Task<Result<ExecutiveDashboardSummaryDto>> GetExecutiveDashboardAsync(
        int campaignId,
        KpiEvaluationMode mode = KpiEvaluationMode.LiveComputed,
        CancellationToken ct = default);

    Task<Result<CustomerCallCenterAuditDto>> GetCustomerAuditAsync(
        int customerId,
        int campaignId,
        CancellationToken ct = default);
}

// Outbound Port (SQL-side aggregations in Infrastructure; in-memory fixture in tests)
public interface ICampaignTelemetryReadPort
{
    Task<CampaignTelemetrySnapshot?> GetCampaignTelemetrySnapshotAsync(
        int campaignId,
        CancellationToken ct = default);
}
```

- **Usage:**
  ```csharp
  var dashboard = await _analytics.GetExecutiveDashboardAsync(campaignId, KpiEvaluationMode.LiveComputed, ct);
  var audit = await _analytics.GetCustomerAuditAsync(customerId, campaignId, ct);
  ```
- **Complexity Hidden Internally:** Server-side SQL aggregation of tracker completions, anti-gaming exclusions, clawbacks, and campaign-scoped average latency (`ICampaignTelemetryReadPort`); live invocation of `IMerchantClassifier.EvaluateMacroF1Async` (KPI-1), `ICupedExperimentEvaluator` (KPI-3), `IDriftAndClusteringService` (KPI-7), free-rider / dormant reactivation ratios (KPI-2, KPI-4), and monotonicity violation counts (KPI-8), with optional short-TTL caching in `FastCached` mode.

---

## Dependency Strategy

| Cluster | Dependency Category | Production Adapter / Implementation | Test Stand-in / Strategy |
| :--- | :--- | :--- | :--- |
| **Cluster 1** (`ICampaignPortfolioEngine`) | **In-process** (ML/MILP) + **Ports & Adapters** (Persistence) | `RobinsonRLearnerUpliftEngine`, `OrToolsMilp`, `EfCoreCampaignLedgerStore` | In-process math/OR-Tools + `InMemoryCampaignLedgerStore` |
| **Cluster 2** (`ISpendTrackingAndRewardLedger`) | **Local-substitutable** + **Ports & Adapters** | `EfCoreCampaignLedgerStore` + `EfCoreUnitOfWork` (SQL `ROWVERSION`) | `InMemoryCampaignLedgerStore` or EF Core InMemory/SQLite |
| **Cluster 3** (`IMerchantClassifier`) | **In-process** (L1/Regex/ML) + **Ports & Adapters** (L2) | `Channel` background writer + `EfCoreMerchantCacheStore` | In-memory `IMerchantCacheStorePort` (no DB required) |
| **Cluster 4** (`ICampaignLedgerStore` / `IUnitOfWork`) | **Ports & Adapters** / **Local-substitutable** | `EfCoreCampaignLedgerStore` using `CompiledQueries` + `AsNoTracking` | Single `InMemoryCampaignLedgerStore` (replaces all `Moq` `IQueryable` hacks) |
| **Cluster 5** (`ICampaignAnalyticsService`) | **In-process** (CUPED/PSI) + **Ports & Adapters** (Telemetry) | SQL-aggregated `EfCoreCampaignTelemetryReadPort` + `CausalAi` evaluators | Deterministic `CampaignTelemetrySnapshot` fixture + real math evaluators |

---

## Testing Strategy

Following **replace, don't layer**: test observable outcomes at the 5 deepened module boundaries rather than testing shallow internal helpers or `IQueryable` mocks.

### New Boundary Tests to Write
1. **Portfolio Optimization Boundary (`ICampaignPortfolioEngine`):**
   - Verify `OptimizeAndAssignAsync` produces XAI rationales whose target/reward text **strictly matches** the MILP-selected bucket, persists assignments + trackers atomically, and leaves DB untouched in `OptimizationExecutionMode.DryRun`.
   - Verify `SelectOfferPreferenceAsync` updates `ThompsonSamplingBanditService` posteriors and triggers `ReconcileTargetAdjustmentAsync` when a lower target causes immediate completion.
2. **Spend & Reward Lifecycle Boundary (`ISpendTrackingAndRewardLedger`):**
   - End-to-end boundary scenarios: valid auth → goal completion → budget allocation → partial/full refund → clawback → budget release, plus `AuthorizationCode` replay protection, symmetric ±30m split-tx blocking, and `TriggerKillSwitchAsync` halting subsequent authorizations and offer switches.
3. **POS Enrichment Boundary (`IMerchantClassifier`):**
   - Multi-MCC isolation (`PAYTR*ONLINE` under `5411` vs `6051`), L1 `ValueTask` fast-path hit, and verification that `ClassifyAsync` **never** triggers synchronous `SaveChangesAsync` on an active `IUnitOfWork`.
4. **Persistence Port Boundary (`ICampaignLedgerStore` + `IUnitOfWork`):**
   - Verify `EfCoreCampaignLedgerStore` executes compiled queries, `AsNoTracking` projections, and transactional rollback on failure against a local EF Core database provider.
5. **Executive KPI Boundary (`ICampaignAnalyticsService`):**
   - Verify all 8 KPIs in `ExecutiveDashboardSummaryDto` are dynamically computed from persisted `CampaignTelemetrySnapshot` data (zero hardcoded KPIs) and scoped strictly to the requested `campaignId`.

### Old Tests to Delete / Consolidate
- Remove `Moq` `IQueryable<T>` / `IAsyncQueryProvider` mock setups in `PosEnrichmentServiceMoqTests` and `DomainServiceContractMoqTests` once replaced by `InMemoryCampaignLedgerStore` and `InMemoryMerchantCacheStore` boundary tests.
- Consolidate redundant isolated helper assertions in `XaiAndBanditPreferenceTests` and `MicroBucketCatalogMonotonicityTests` into the `ICampaignPortfolioEngine` boundary suite while keeping pure mathematical edge-case tests (`CupedExperimentEvaluator`, `InflationPsiDriftMonitor`).

---

## Implementation Recommendations

1. **Module Ownership:**
   - **Optimization & Causal Allocation** owns feature hydration, uplift estimation, MILP solving, post-allocation XAI explanation, offer bundling, and bandit feedback.
   - **Spend & Reward Ledger** is the sole authority for spend accumulation, anti-gaming rules, campaign operational status checks, budget allocation/release, and reward fulfillment/clawback.
   - **POS Classifier** owns L1/L2 cache coherence and 3-layer classification without ever holding or flushing a transactional `DbContext`.
   - **Persistence Port** owns all EF Core specifics (`DbSet`, `AsNoTracking`, `EF.CompileAsyncQuery`, `EnableRetryOnFailure`, `TransactionScope`); `Application`, `PosEnrichment`, and `CausalAi` must have zero `Microsoft.EntityFrameworkCore` package references.
   - **Analytics Service** owns live 8-KPI scorecard assembly from server-aggregated telemetry snapshots.
2. **What Each Module Must Hide:**
   - Hide `IQueryable<T>`, `IAsyncQueryProvider`, `SaveChangesAsync` timing, `PredictionEngine` pooling/disposal, L1 eviction policies, background `Channel` write-behind queues, and XAI JSON serialization formats behind the deepened boundaries.
3. **Caller Migration Path:**
   - **Step 1:** Introduce the outbound ports (`ICampaignLedgerStore`, `IUnitOfWork`, `IMerchantCacheStorePort`, `ICampaignTelemetryReadPort`) and their EF Core / background-channel adapters in `Infrastructure`.
   - **Step 2:** Refactor `PosEnrichment` to implement `IMerchantClassifier` and drop its EF Core dependency.
   - **Step 3:** Consolidate `ISpendTrackingAndRewardLedger` and `ICampaignPortfolioEngine` (moving XAI generation post-MILP and wiring Bandit feedback).
   - **Step 4:** Wire `ICampaignAnalyticsService` to real telemetry + `CausalAi` evaluators, then switch `CampaignEngineController` and Blazor `Home.razor` to the new facades and remove `ICausalCampaignDbContext`.
