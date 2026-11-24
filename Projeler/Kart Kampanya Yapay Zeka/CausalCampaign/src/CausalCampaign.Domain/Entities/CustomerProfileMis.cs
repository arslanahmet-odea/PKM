using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.Constants;

namespace CausalCampaign.Domain.Entities;

/// <summary>
/// Rich domain entity representing the CUSTOMER_PROFILE_MIS table (card profile, available credit limit, and MIS profitability).
/// </summary>
public class CustomerProfileMis : BaseEntity
{
    public int CustomerId { get; set; }
    public string CustomerExternalNo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string CardNumberMasked { get; set; } = string.Empty;
    public string PrestigeSegment { get; set; } = "Standard";
    public decimal TotalCreditLimit { get; set; }
    public decimal AvailableCreditLimit { get; set; }
    public decimal MisMonthlyNetProfit { get; set; }
    public decimal HistoricalBaselineSpend { get; set; }
    public decimal RealBaselineSpendIndex { get; set; }
    public string SpendElasticityPersona { get; set; } = "Persuadable";
    public bool IsDormant { get; set; }
    public int DaysSinceLastTx { get; set; }
    public bool HasMarketingConsent { get; set; } = true;
    public string PreferredRewardType { get; set; } = "RewardPoints";
    public int BehavioralClusterId { get; set; } = 1;

    /// <summary>
    /// Calculates the maximum spend target that can be assigned to the customer (80% of available credit limit).
    /// </summary>
    public decimal GetMaxAllowedTargetAmount()
    {
        return Math.Round(Math.Max(0m, AvailableCreditLimit) * AntiGamingConstants.MaxAvailableCreditLimitTargetRatio, 2);
    }

    /// <summary>
    /// Checks customer campaign eligibility based on marketing consent, MIS profitability threshold, and available credit limit.
    /// </summary>
    public Result CheckCampaignEligibility(decimal minMisProfitThreshold)
    {
        if (!HasMarketingConsent)
        {
            return Result.Failure(DomainError.Validation(
                "CUSTOMER_NO_MARKETING_CONSENT",
                $"Customer ({CustomerExternalNo}) does not have active marketing communication consent."));
        }

        if (MisMonthlyNetProfit < minMisProfitThreshold)
        {
            return Result.Failure(DomainError.Validation(
                "CUSTOMER_BELOW_MIS_THRESHOLD",
                $"Customer monthly MIS net profit ({MisMonthlyNetProfit:N2} TRY) is below the campaign minimum threshold ({minMisProfitThreshold:N2} TRY)."));
        }

        if (AvailableCreditLimit <= 0m)
        {
            return Result.Failure(DomainError.Validation(
                "CUSTOMER_NO_AVAILABLE_LIMIT",
                $"Customer ({CustomerExternalNo}) has no available credit card limit."));
        }

        return Result.Success();
    }
}
