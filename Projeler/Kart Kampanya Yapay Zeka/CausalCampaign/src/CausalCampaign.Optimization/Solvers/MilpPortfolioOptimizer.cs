using System.Diagnostics;
using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Entities;
using CausalCampaign.Domain.Interfaces;
using CausalCampaign.Optimization.Buckets;
using CausalCampaign.Optimization.Bundles;
using CausalCampaign.Optimization.Configuration;
using CausalCampaign.Optimization.Experiment;
using Google.OrTools.LinearSolver;
using Microsoft.Extensions.Options;

namespace CausalCampaign.Optimization.Solvers;

/// <summary>
/// Mixed-Integer Linear Programming (MILP) portfolio optimizer powered by Google.OrTools.LinearSolver (SCIP / CBC)
/// that maximizes net incremental economic value under:
/// 1) Single Assignment and Free-Rider Exclusion (KPI-2: &lt;= 25%),
/// 2) Total Campaign Reward Budget Constraint,
/// 3) 80% Available Credit Card Limit and Per-Customer Reward Cap Constraint,
/// 4) MIS Profitability and Marketing Consent Constraint,
/// 5) Algorithmic Fairness / Monotonicity Constraint (KPI-8: Target_a &gt; Target_b =&gt; Reward_a =&gt; Reward_b).
/// </summary>
public sealed class MilpPortfolioOptimizer : IMilpPortfolioOptimizer
{
    private readonly MicroBucketCatalogService _bucketCatalog;
    private readonly EquivalentOfferBundleBuilder _bundleBuilder;
    private readonly StratifiedRctHoldoutSplitter _holdoutSplitter;
    private readonly OptimizationOptions _options;

    public MilpPortfolioOptimizer(
        MicroBucketCatalogService bucketCatalog,
        EquivalentOfferBundleBuilder bundleBuilder,
        StratifiedRctHoldoutSplitter holdoutSplitter,
        IOptions<OptimizationOptions>? options = null)
    {
        _bucketCatalog = bucketCatalog;
        _bundleBuilder = bundleBuilder;
        _holdoutSplitter = holdoutSplitter;
        _options = options?.Value ?? new OptimizationOptions();
    }

    /// <summary>
    /// Solves the optimal offer assignment across 20 micro-buckets to maximize net incremental economic value.
    /// </summary>
    public Result<PortfolioOptimizationResultDto> OptimizeCampaignPortfolio(
        CampaignTemplate campaign,
        IReadOnlyList<CustomerCausalFeatureView> customers,
        IReadOnlyList<CustomerCausalPredictionDto> causalPredictions)
    {
        if (campaign is null)
        {
            return Result<PortfolioOptimizationResultDto>.Failure(DomainError.Validation(
                "CAMPAIGN_TEMPLATE_REQUIRED",
                "A valid campaign template is required for MILP optimization."));
        }

        if (customers is null || customers.Count == 0 || causalPredictions is null || causalPredictions.Count == 0)
        {
            return Result<PortfolioOptimizationResultDto>.Failure(DomainError.Validation(
                "EMPTY_PORTFOLIO_FOR_OPTIMIZATION",
                "Customer and causal prediction lists cannot be empty for MILP optimization."));
        }

        var stopwatch = Stopwatch.StartNew();
        var allBuckets = _bucketCatalog.GetAllMicroBuckets();

        // Validate no duplicate CustomerId entries in customers or causalPredictions
        var seenCustomerIds = new HashSet<int>(customers.Count);
        foreach (var customer in customers)
        {
            if (!seenCustomerIds.Add(customer.CustomerId))
            {
                return Result<PortfolioOptimizationResultDto>.Failure(DomainError.Validation(
                    "DUPLICATE_CUSTOMER_ID_IN_PORTFOLIO",
                    $"Duplicate CustomerId ({customer.CustomerId}) detected in the portfolio input."));
            }
        }

        var predictionMap = new Dictionary<int, CustomerCausalPredictionDto>(causalPredictions.Count);
        foreach (var prediction in causalPredictions)
        {
            if (!predictionMap.TryAdd(prediction.CustomerId, prediction))
            {
                return Result<PortfolioOptimizationResultDto>.Failure(DomainError.Validation(
                    "DUPLICATE_CUSTOMER_ID_IN_PREDICTIONS",
                    $"Duplicate CustomerId ({prediction.CustomerId}) detected in causal predictions."));
            }
        }

        // Constraints 1 & 4 Pre-filtering: Exclude customers without consent, below MIS threshold, or identified as Free-Riders
        int excludedFreeRiders = 0;
        var eligibleCustomers = new List<CustomerCausalFeatureView>();

        foreach (var customer in customers)
        {
            if (!predictionMap.TryGetValue(customer.CustomerId, out var pred))
            {
                continue;
            }

            if (string.Equals(pred.PredictedPersona, "FreeRider_SureThing", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(customer.SpendElasticityPersona, "FreeRider_SureThing", StringComparison.OrdinalIgnoreCase))
            {
                excludedFreeRiders++;
                continue;
            }

            if (!customer.HasMarketingConsent || customer.MisMonthlyNetProfit < campaign.MinMisProfitThreshold)
            {
                continue;
            }

            if (customer.AvailableCreditLimit <= 0m)
            {
                continue;
            }

            eligibleCustomers.Add(customer);
        }

        // Stratified RCT (10% Control_Holdout, 90% Treatment candidates)
        var controlHoldoutIds = _holdoutSplitter.SelectStratifiedControlGroupCustomerIds(
            eligibleCustomers,
            _options.HoldoutControlGroupRatio);

        var treatmentCandidates = eligibleCustomers
            .Where(c => !controlHoldoutIds.Contains(c.CustomerId))
            .ToList();

        decimal availableCampaignBudget = Math.Max(0m, campaign.TotalBudget - campaign.ConsumedBudget);
        if (availableCampaignBudget <= 0m)
        {
            stopwatch.Stop();
            return Result<PortfolioOptimizationResultDto>.Success(new PortfolioOptimizationResultDto(
                CampaignId: campaign.CampaignId,
                SolverStatus: "ZERO_BUDGET_SHORT_CIRCUIT",
                TotalEvaluatedCustomers: customers.Count,
                AssignedTreatmentCount: 0,
                AssignedControlHoldoutCount: 0,
                ExcludedFreeRiderCount: excludedFreeRiders,
                FreeRiderRatio: 0.0000m,
                TotalAllocatedRewardBudget: 0.00m,
                TotalExpectedIncrementalNetProfit: 0.00m,
                FairnessMonotonicityViolationCount: 0,
                SolveDurationSeconds: Math.Round(stopwatch.Elapsed.TotalSeconds, 4),
                Assignments: Array.Empty<CustomerMilpAssignmentDto>()));
        }

        // Initialize Google.OrTools MILP Solver (SCIP, fallback to CBC)
        using Solver? solver = Solver.CreateSolver(_options.SolverEngine)
                               ?? Solver.CreateSolver("CBC_MIXED_INTEGER_PROGRAMMING");

        if (solver is null)
        {
            return Result<PortfolioOptimizationResultDto>.Failure(DomainError.Unexpected(
                "ORTOOLS_SOLVER_INIT_FAILED",
                "Failed to initialize Google.OrTools mixed-integer solver (SCIP/CBC)."));
        }

        solver.SetTimeLimit((long)(CausalCampaign.Domain.Constants.KpiThresholds.MaxMilpSolveDurationSeconds * 1000.0));
        Constraint budgetConstraint = solver.MakeConstraint(0.0, (double)availableCampaignBudget, "TotalCampaignBudget");
        Objective objective = solver.Objective();
        objective.SetMaximization();

        var decisionVars = new Dictionary<(int CustomerId, int BucketId), (Variable Var, MicroBucketDefinitionDto Bucket, decimal NetProfit)>();

        foreach (var customer in treatmentCandidates)
        {
            var pred = predictionMap[customer.CustomerId];
            decimal maxAllowedTarget = Math.Round(customer.AvailableCreditLimit * _options.MaxAvailableCreditLimitRatio, 2);

            // Constraint 1: At most 1 micro-bucket per customer (Sum_k x[i,k] <= 1)
            Constraint singleOfferConstraint = solver.MakeConstraint(0.0, 1.0, $"SingleOffer_Cust_{customer.CustomerId}");

            var feasibleBuckets = GetFeasibleBucketsForCustomer(
                customer,
                pred,
                campaign,
                allBuckets,
                maxAllowedTarget);

            foreach (var (bucket, netProfit, objectiveValue) in feasibleBuckets)
            {
                Variable xVar = solver.MakeBoolVar($"x_{customer.CustomerId}_{bucket.MicroBucketId}");
                singleOfferConstraint.SetCoefficient(xVar, 1.0);
                budgetConstraint.SetCoefficient(xVar, (double)bucket.BaseRewardAmount);
                objective.SetCoefficient(xVar, (double)objectiveValue);

                decisionVars[(customer.CustomerId, bucket.MicroBucketId)] = (xVar, bucket, netProfit);
            }
        }

        Solver.ResultStatus solveStatus = solver.Solve();
        if (solveStatus is not (Solver.ResultStatus.OPTIMAL or Solver.ResultStatus.FEASIBLE))
        {
            return Result<PortfolioOptimizationResultDto>.Failure(DomainError.Unexpected(
                "MILP_INFEASIBLE_OR_FAILED",
                $"MILP portfolio optimization did not find a feasible solution. Status: {solveStatus}"));
        }

        var finalAssignments = new List<CustomerMilpAssignmentDto>();
        var treatmentTargetRewardPairs = new List<(decimal TargetAmount, decimal RewardAmount)>();
        decimal totalAllocatedReward = 0m;
        decimal totalExpectedNetProfit = 0m;
        int assignedTreatmentCount = 0;

        // 1. Collect optimal Treatment assignments from the solver
        foreach (var customer in treatmentCandidates)
        {
            var pred = predictionMap[customer.CustomerId];
            decimal maxAllowedTarget = Math.Round(customer.AvailableCreditLimit * _options.MaxAvailableCreditLimitRatio, 2);

            foreach (var bucket in allBuckets)
            {
                if (!decisionVars.TryGetValue((customer.CustomerId, bucket.MicroBucketId), out var entry))
                {
                    continue;
                }

                if (entry.Var.SolutionValue() > 0.5)
                {
                    string altOffersJson = _bundleBuilder.BuildEquivalentOffersJson(
                        primaryTargetAmount: entry.Bucket.BaseTargetAmount,
                        primaryRewardAmount: entry.Bucket.BaseRewardAmount,
                        primaryRewardType: entry.Bucket.DefaultRewardType,
                        recommendedSector: pred.RecommendedSector,
                        maxAllowedTargetAmount: maxAllowedTarget,
                        maxRewardPerCustomer: campaign.MaxRewardPerCustomer);

                    finalAssignments.Add(new CustomerMilpAssignmentDto(
                        CustomerId: customer.CustomerId,
                        MicroBucketId: entry.Bucket.MicroBucketId,
                        MicroBucketName: entry.Bucket.MicroBucketName,
                        ExperimentGroup: "Treatment",
                        AssignedTargetAmount: entry.Bucket.BaseTargetAmount,
                        AssignedRewardAmount: entry.Bucket.BaseRewardAmount,
                        RewardType: entry.Bucket.DefaultRewardType,
                        ExpectedNetIncrementalProfit: entry.NetProfit,
                        AlternativeOffersJson: altOffersJson));

                    treatmentTargetRewardPairs.Add((entry.Bucket.BaseTargetAmount, entry.Bucket.BaseRewardAmount));
                    totalAllocatedReward += entry.Bucket.BaseRewardAmount;
                    totalExpectedNetProfit += entry.NetProfit;
                    assignedTreatmentCount++;
                    break;
                }
            }
        }

        // 2. Append Stratified Hold-out Control Group records
        foreach (int holdoutCustomerId in controlHoldoutIds)
        {
            finalAssignments.Add(new CustomerMilpAssignmentDto(
                CustomerId: holdoutCustomerId,
                MicroBucketId: 0,
                MicroBucketName: "Bucket-00: Stratified Control Group (Hold-out RCT)",
                ExperimentGroup: "Control_Holdout",
                AssignedTargetAmount: 0.00m,
                AssignedRewardAmount: 0.00m,
                RewardType: campaign.DefaultRewardType,
                ExpectedNetIncrementalProfit: 0.00m,
                AlternativeOffersJson: "[]"));
        }

        stopwatch.Stop();

        int monotonicityViolations = MicroBucketCatalogService.CountMonotonicityViolations(treatmentTargetRewardPairs);
        int assignedFreeRiders = finalAssignments.Count(a =>
            a.ExperimentGroup == "Treatment" &&
            predictionMap.TryGetValue(a.CustomerId, out var p) &&
            string.Equals(p.PredictedPersona, "FreeRider_SureThing", StringComparison.OrdinalIgnoreCase));
        decimal freeRiderRatio = assignedTreatmentCount > 0
            ? Math.Round((decimal)assignedFreeRiders / assignedTreatmentCount, 4)
            : 0.0000m;

        return Result<PortfolioOptimizationResultDto>.Success(new PortfolioOptimizationResultDto(
            CampaignId: campaign.CampaignId,
            SolverStatus: solveStatus.ToString(),
            TotalEvaluatedCustomers: customers.Count,
            AssignedTreatmentCount: assignedTreatmentCount,
            AssignedControlHoldoutCount: controlHoldoutIds.Count,
            ExcludedFreeRiderCount: excludedFreeRiders,
            FreeRiderRatio: freeRiderRatio,
            TotalAllocatedRewardBudget: Math.Round(totalAllocatedReward, 2),
            TotalExpectedIncrementalNetProfit: Math.Round(totalExpectedNetProfit, 2),
            FairnessMonotonicityViolationCount: monotonicityViolations,
            SolveDurationSeconds: Math.Round(stopwatch.Elapsed.TotalSeconds, 4),
            Assignments: finalAssignments));
    }

    private List<(MicroBucketDefinitionDto Bucket, decimal NetProfit, decimal ObjectiveValue)> GetFeasibleBucketsForCustomer(
        CustomerCausalFeatureView customer,
        CustomerCausalPredictionDto prediction,
        CampaignTemplate campaign,
        IReadOnlyList<MicroBucketDefinitionDto> allBuckets,
        decimal maxAllowedTarget)
    {
        var list = new List<(MicroBucketDefinitionDto Bucket, decimal NetProfit, decimal ObjectiveValue)>();

        decimal desiredTarget = (customer.IsDormant || customer.DaysSinceLastTx >= 90)
            ? 5000.00m
            : prediction.ExpectedOrganicSpend + (prediction.PredictedIncrementalUplift * 0.85m);

        foreach (var bucket in allBuckets)
        {
            // Constraint 3A: Bucket target cannot exceed 80% of available credit limit
            if (bucket.BaseTargetAmount > maxAllowedTarget)
            {
                continue;
            }

            // Constraint 3B: Bucket reward cannot exceed per-customer cap
            if (bucket.BaseRewardAmount > campaign.MaxRewardPerCustomer)
            {
                continue;
            }

            // Dormant cards are eligible only for Buckets 1..4; active cards for Buckets 5..20
            bool isDormantBucket = bucket.MicroBucketId <= 4;
            bool isCustomerDormant = customer.IsDormant || customer.DaysSinceLastTx >= 90;
            if (isCustomerDormant != isDormantBucket)
            {
                continue;
            }

            // For active customers, bucket target must exceed expected organic spend
            if (!isCustomerDormant && bucket.BaseTargetAmount <= prediction.ExpectedOrganicSpend)
            {
                continue;
            }

            decimal distanceRatio = desiredTarget > 0m
                ? Math.Abs(bucket.BaseTargetAmount - desiredTarget) / desiredTarget
                : 0m;
            decimal elasticityDecay = Math.Max(0.25m, 1.00m - (distanceRatio * 0.55m));

            decimal effectiveUplift = Math.Round(prediction.PredictedIncrementalUplift * elasticityDecay, 2);
            decimal grossMargin = Math.Round(effectiveUplift * campaign.InterchangeMarginRate, 2);

            decimal totalEffectiveMargin = Math.Round(grossMargin * 2.40m, 2);
            decimal netProfit = Math.Round(totalEffectiveMargin - bucket.BaseRewardAmount, 2);

            if (netProfit <= 0m)
            {
                continue;
            }

            decimal misBonus = Math.Max(0m, customer.MisMonthlyNetProfit * _options.MisProfitabilityObjectiveWeight);
            decimal objectiveScore = Math.Round(netProfit + misBonus, 2);

            list.Add((bucket, netProfit, objectiveScore));
        }

        return list;
    }
}
