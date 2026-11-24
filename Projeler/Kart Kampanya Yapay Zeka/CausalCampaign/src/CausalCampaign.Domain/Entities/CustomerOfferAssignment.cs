using System.Text.Json;
using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.ValueObjects;

namespace CausalCampaign.Domain.Entities;

/// <summary>
/// Rich domain entity representing the CUSTOMER_OFFER_ASSIGNMENT table (R-Learner + MILP offer assignment and preference learning).
/// </summary>
public class CustomerOfferAssignment
{
    private static readonly JsonSerializerOptions CaseInsensitiveJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public int AssignmentId { get; set; }
    public int CampaignId { get; set; }
    public int CustomerId { get; set; }
    public int MicroBucketId { get; set; }
    public string MicroBucketName { get; set; } = string.Empty;
    public string ExperimentGroup { get; set; } = "Treatment";
    public decimal ExpectedOrganicSpend { get; set; }
    public decimal PredictedUpliftSpend { get; set; }
    public decimal PropensityScore { get; set; }
    public decimal ReactivationProbability { get; set; }
    public decimal AssignedTargetAmount { get; set; }
    public decimal AssignedRewardAmount { get; set; }
    public string RewardType { get; set; } = "RewardPoints";
    public decimal ExpectedNetIncrementalProfit { get; set; }
    public string XaiReasonText { get; set; } = string.Empty;
    public string XaiTopFeaturesJson { get; set; } = "[]";
    public string AlternativeOffersJson { get; set; } = "[]";
    public byte CustomerSelectedOfferIndex { get; set; } = 1;
    public bool IsApprovedByHuman { get; set; } = true;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Safely deserializes the JSON array of 3 equivalent-profit alternative offer options.
    /// </summary>
    public IReadOnlyList<AlternativeOfferOption> GetAlternativeOfferOptions()
    {
        if (string.IsNullOrWhiteSpace(AlternativeOffersJson))
        {
            return Array.Empty<AlternativeOfferOption>();
        }

        try
        {
            var options = JsonSerializer.Deserialize<List<AlternativeOfferOption>>(
                AlternativeOffersJson,
                CaseInsensitiveJsonOptions);

            return options is not null ? options : Array.Empty<AlternativeOfferOption>();
        }
        catch (JsonException)
        {
            return Array.Empty<AlternativeOfferOption>();
        }
    }

    /// <summary>
    /// Applies the customer's choice among the 3 equivalent offer options (Active Preference Learning).
    /// </summary>
    /// <param name="selectedIndex">Selected offer option index (1, 2, or 3).</param>
    public Result<AlternativeOfferOption> SelectAlternativeOffer(byte selectedIndex)
    {
        if (string.Equals(ExperimentGroup, "Control_Holdout", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AlternativeOfferOption>.Failure(DomainError.Validation(
                "CONTROL_GROUP_OFFER_SELECTION_NOT_ALLOWED",
                "Customers in the Control_Holdout experiment cohort cannot select an alternative campaign offer."));
        }

        if (selectedIndex < 1 || selectedIndex > 3)
        {
            return Result<AlternativeOfferOption>.Failure(DomainError.Validation(
                "INVALID_OFFER_OPTION_INDEX",
                "Selected alternative offer index must be 1, 2, or 3."));
        }

        var options = GetAlternativeOfferOptions();
        var chosen = options.FirstOrDefault(o => o.Index == selectedIndex);
        if (chosen is null)
        {
            return Result<AlternativeOfferOption>.Failure(DomainError.NotFound(
                "ALTERNATIVE_OFFER_NOT_FOUND",
                $"Alternative offer option with index ({selectedIndex}) was not found."));
        }

        if (chosen.Target <= 0m || chosen.Reward <= 0m || string.IsNullOrWhiteSpace(chosen.RewardType))
        {
            return Result<AlternativeOfferOption>.Failure(DomainError.Validation(
                "INVALID_ALTERNATIVE_OFFER_PAYLOAD",
                "The selected alternative offer contains invalid target, reward, or reward type values."));
        }

        CustomerSelectedOfferIndex = chosen.Index;
        AssignedTargetAmount = chosen.Target;
        AssignedRewardAmount = chosen.Reward;
        RewardType = chosen.RewardType;

        return Result<AlternativeOfferOption>.Success(chosen);
    }
}
