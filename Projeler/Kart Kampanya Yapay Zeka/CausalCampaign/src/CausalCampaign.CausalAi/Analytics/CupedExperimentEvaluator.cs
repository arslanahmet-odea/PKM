using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.Constants;
using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Interfaces;
using MathNet.Numerics.Statistics;

namespace CausalCampaign.CausalAi.Analytics;

/// <summary>
/// KPI-3 (Incremental ROI =&gt; 1.35x): Verifies net incremental economic value ((Incremental Spend * Margin) - Reward)
/// using Stratified Randomized Controlled Trials (Stratified RCT) and CUPED variance reduction.
/// </summary>
public sealed class CupedExperimentEvaluator : ICupedExperimentEvaluator
{
    /// <summary>
    /// Uses pre-experiment baseline spend (X_pre) as a control variate to compute CUPED-adjusted incremental spend
    /// and Incremental ROI per unit reward cost.
    /// </summary>
    public Result<CupedEvaluationResultDto> EvaluateCampaignIncrementalRoi(
        int campaignId,
        decimal marginRate,
        IReadOnlyList<(decimal PreExperimentSpend, decimal PostExperimentSpend, decimal RewardCost, bool IsTreatment)> observations)
    {
        if (observations is null || observations.Count < 4)
        {
            return Result<CupedEvaluationResultDto>.Failure(DomainError.Validation(
                "INSUFFICIENT_EXPERIMENT_OBSERVATIONS",
                "At least 4 observations are required for CUPED experiment analysis."));
        }

        if (marginRate <= 0m)
        {
            return Result<CupedEvaluationResultDto>.Failure(DomainError.Validation(
                "INVALID_MARGIN_RATE",
                "Interchange margin rate must be greater than zero."));
        }

        if (observations.Any(o => o.RewardCost < 0m))
        {
            return Result<CupedEvaluationResultDto>.Failure(DomainError.Validation(
                "NEGATIVE_REWARD_COST",
                "Reward cost cannot be negative in CUPED experiment observations."));
        }

        var treatmentGroup = observations.Where(o => o.IsTreatment).ToList();
        var controlGroup = observations.Where(o => !o.IsTreatment).ToList();

        if (treatmentGroup.Count < 2 || controlGroup.Count < 2)
        {
            return Result<CupedEvaluationResultDto>.Failure(DomainError.Validation(
                "MISSING_TREATMENT_OR_CONTROL_COHORT",
                "At least 2 customers in both Treatment and Control_Holdout cohorts are required for CUPED calculation."));
        }

        double[] allPreX = observations.Select(o => (double)o.PreExperimentSpend).ToArray();
        double meanPreX = allPreX.Mean();
        double meanTreatPost = treatmentGroup.Select(o => (double)o.PostExperimentSpend).Mean();
        double meanCtrlPost = controlGroup.Select(o => (double)o.PostExperimentSpend).Mean();

        // Compute Theta from covariance between within-group centered post-spend Y and pre-experiment X
        double[] demeanedPostY = observations
            .Select(o => (double)o.PostExperimentSpend - (o.IsTreatment ? meanTreatPost : meanCtrlPost))
            .ToArray();
        double[] demeanedPreX = observations
            .Select(o => (double)o.PreExperimentSpend - meanPreX)
            .ToArray();

        double varPreX = demeanedPreX.Variance();
        double covPostPre = ArrayStatistics.Covariance(demeanedPostY, demeanedPreX);

        // Theta = Cov(Y_within, X_pre) / Var(X_pre)
        double theta = varPreX > 1e-6 ? covPostPre / varPreX : 0.0;

        double[] cupedDemeanedY = new double[observations.Count];
        for (int i = 0; i < observations.Count; i++)
        {
            cupedDemeanedY[i] = demeanedPostY[i] - (theta * demeanedPreX[i]);
        }

        double rawWithinVariance = demeanedPostY.Variance();
        double cupedWithinVariance = cupedDemeanedY.Variance();
        decimal varianceReductionPct = rawWithinVariance > 1e-6
            ? Math.Round((decimal)Math.Clamp((1.0 - (cupedWithinVariance / rawWithinVariance)) * 100.0, 0.0, 99.0), 2)
            : 0.00m;

        double meanCupedTreatment = treatmentGroup
            .Select(o => (double)o.PostExperimentSpend - (theta * ((double)o.PreExperimentSpend - meanPreX)))
            .Mean();

        double meanCupedControl = controlGroup
            .Select(o => (double)o.PostExperimentSpend - (theta * ((double)o.PreExperimentSpend - meanPreX)))
            .Mean();

        // Do not mask negative lift ("Sleeping Dogs" / harmful campaigns)
        decimal incrementalSpendPerCustomer = Math.Round((decimal)(meanCupedTreatment - meanCupedControl), 2);
        decimal totalIncrementalGrossMargin = Math.Round(incrementalSpendPerCustomer * treatmentGroup.Count * marginRate, 2);
        decimal totalRewardCost = Math.Round(treatmentGroup.Sum(o => o.RewardCost), 2);
        decimal totalIncrementalNetProfit = Math.Round(totalIncrementalGrossMargin - totalRewardCost, 2);

        decimal incrementalRoi = totalRewardCost > 0m
            ? Math.Round(totalIncrementalGrossMargin / totalRewardCost, 4)
            : 0.0000m;

        double clampedTheta = Math.Clamp(theta, -1_000_000.0, 1_000_000.0);

        return Result<CupedEvaluationResultDto>.Success(new CupedEvaluationResultDto(
            CampaignId: campaignId,
            TreatmentCustomerCount: treatmentGroup.Count,
            ControlCustomerCount: controlGroup.Count,
            ThetaCovariateWeight: Math.Round((decimal)clampedTheta, 4),
            VarianceReductionPercentage: varianceReductionPct,
            CupedIncrementalSpendPerCustomer: incrementalSpendPerCustomer,
            TotalIncrementalNetProfit: totalIncrementalNetProfit,
            TotalRewardCost: totalRewardCost,
            IncrementalRoi: incrementalRoi,
            MeetsKpi3Threshold: incrementalRoi >= KpiThresholds.MinIncrementalRoi));
    }
}
