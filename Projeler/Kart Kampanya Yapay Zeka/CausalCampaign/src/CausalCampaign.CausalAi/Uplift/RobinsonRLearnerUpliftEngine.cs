using CausalCampaign.CausalAi.Bandit;
using CausalCampaign.CausalAi.Configuration;
using CausalCampaign.CausalAi.Recommendation;
using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Entities;
using CausalCampaign.Domain.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace CausalCampaign.CausalAi.Uplift;

/// <summary>
/// Causal machine learning engine that estimates Individual Treatment Effect (ITE / Uplift)
/// using a two-stage Robinson Decomposition R-Learner with Empirical Bayes Shrinkage,
/// and identifies Free-Rider customers.
/// </summary>
public sealed class RobinsonRLearnerUpliftEngine : ICausalUpliftEngine
{
    private readonly MLContext _mlContext;
    private readonly ITransformer _stage1OrganicBaselineModel;
    private readonly ITransformer _stage2ResidualUpliftModel;
    private readonly DormantCardReactivationModel _dormantModel;
    private readonly MatrixFactorizationSectorRecommender _sectorRecommender;
    private readonly ThompsonSamplingBanditService _banditService;
    private readonly IXaiExplanationService _xaiService;
    private readonly CausalAiOptions _options;
    private readonly object _lock = new();

    public RobinsonRLearnerUpliftEngine(
        DormantCardReactivationModel dormantModel,
        MatrixFactorizationSectorRecommender sectorRecommender,
        ThompsonSamplingBanditService banditService,
        IXaiExplanationService xaiService,
        IOptions<CausalAiOptions>? options = null)
    {
        _dormantModel = dormantModel;
        _sectorRecommender = sectorRecommender;
        _banditService = banditService;
        _xaiService = xaiService;
        _options = options?.Value ?? new CausalAiOptions();

        _mlContext = new MLContext(seed: 42);
        (_stage1OrganicBaselineModel, _stage2ResidualUpliftModel) = TrainTwoStageRobinsonRLearnerModels(_mlContext, _options);
    }

    /// <summary>
    /// Predicts Stage 1 expected organic spend, Stage 2 R-Learner + Empirical Bayes shrunk incremental uplift,
    /// Free-Rider persona classification, and XAI explanation across the customer portfolio.
    /// </summary>
    public Result<IReadOnlyList<CustomerCausalPredictionDto>> PredictPortfolioCausalUplift(
        IReadOnlyList<CustomerCausalFeatureView> customerFeatures,
        CampaignTemplate campaign)
    {
        if (campaign is null)
        {
            return Result<IReadOnlyList<CustomerCausalPredictionDto>>.Failure(DomainError.Validation(
                "CAMPAIGN_TEMPLATE_REQUIRED",
                "A valid campaign template is required for causal uplift prediction."));
        }

        if (customerFeatures is null || customerFeatures.Count == 0)
        {
            return Result<IReadOnlyList<CustomerCausalPredictionDto>>.Failure(DomainError.Validation(
                "EMPTY_CUSTOMER_FEATURE_LIST",
                "At least 1 customer feature record is required for causal uplift prediction."));
        }

        PredictionEngine<CausalCustomerInput, RegressionScorePrediction> organicEngine;
        PredictionEngine<CausalCustomerInput, RegressionScorePrediction> upliftEngine;

        lock (_lock)
        {
            organicEngine = _mlContext.Model.CreatePredictionEngine<CausalCustomerInput, RegressionScorePrediction>(_stage1OrganicBaselineModel);
            upliftEngine = _mlContext.Model.CreatePredictionEngine<CausalCustomerInput, RegressionScorePrediction>(_stage2ResidualUpliftModel);
        }

        using var _organicDisposable = organicEngine;
        using var _upliftDisposable = upliftEngine;

        var results = new List<CustomerCausalPredictionDto>(customerFeatures.Count);

        foreach (var customer in customerFeatures)
        {
            var mlInput = MapToCausalInput(customer);
            bool isDormantCustomer = customer.IsDormant || customer.DaysSinceLastTx >= 90;

            // Stage 1: Expected Organic Spend m_hat(X) and Clipped Propensity Score e_hat(X)
            float rawOrganicScore = organicEngine.Predict(mlInput).Score;
            decimal expectedOrganicSpend = isDormantCustomer
                ? Math.Round(customer.RealBaselineSpendIndex, 2)
                : Math.Round(Math.Max(customer.RealBaselineSpendIndex, (decimal)rawOrganicScore), 2);

            decimal propensityScore = EstimateClippedPropensityScore(customer);

            // Stage 2: Robinson R-Learner Residual Uplift Prediction + Empirical Bayes Shrinkage
            decimal finalUpliftSpend;
            decimal reactivationProb = 0.0000m;
            string predictedPersona;

            if (isDormantCustomer)
            {
                (reactivationProb, finalUpliftSpend) = _dormantModel.PredictDormantReactivation(customer);
                predictedPersona = "Dormant_Sleeper";
            }
            else
            {
                float rawUpliftScore = upliftEngine.Predict(mlInput).Score;
                decimal unshrunkUplift = Math.Max(0m, (decimal)rawUpliftScore);

                // Empirical Bayes Shrinkage: lambda_i = n_i / (n_i + kappa)
                decimal obsCount = Math.Max(1m, customer.ValidTxCount);
                decimal lambda = obsCount / (obsCount + _options.EmpiricalBayesPriorKappa);
                decimal segmentPriorUplift = GetSegmentPriorUplift(customer.PrestigeSegment, expectedOrganicSpend);

                finalUpliftSpend = Math.Round((lambda * unshrunkUplift) + ((1m - lambda) * segmentPriorUplift), 2);

                // Free-Rider (Sure Thing) and LostCause evaluation
                bool isFreeRider = string.Equals(customer.SpendElasticityPersona, "FreeRider_SureThing", StringComparison.OrdinalIgnoreCase)
                                   || (expectedOrganicSpend >= _options.FreeRiderOrganicSpendThreshold &&
                                       (expectedOrganicSpend > 0m && (finalUpliftSpend / expectedOrganicSpend) <= _options.FreeRiderMaxRelativeUpliftRatio));

                if (isFreeRider)
                {
                    finalUpliftSpend = Math.Round(expectedOrganicSpend * 0.025m, 2);
                    predictedPersona = "FreeRider_SureThing";
                }
                else if (customer.MisMonthlyNetProfit < campaign.MinMisProfitThreshold || !customer.HasMarketingConsent)
                {
                    finalUpliftSpend = 0.00m;
                    predictedPersona = "LostCause";
                }
                else
                {
                    finalUpliftSpend = Math.Max(finalUpliftSpend, Math.Round(expectedOrganicSpend * 0.42m, 2));
                    predictedPersona = "Persuadable";
                }
            }

            string recommendedSector = _sectorRecommender.RecommendNextBestSector(customer);
            string preferredReward = _banditService.SelectRewardTypeWithBandit(customer.CustomerId, customer.PreferredRewardType);

            decimal previewTarget = isDormantCustomer
                ? 5000.00m
                : Math.Round((expectedOrganicSpend + (finalUpliftSpend * 0.85m)) / 100m, 0) * 100m;
            decimal previewReward = Math.Round(Math.Min(campaign.MaxRewardPerCustomer, finalUpliftSpend * campaign.InterchangeMarginRate * 0.55m), 0);

            var (explanationText, topFeatures) = _xaiService.GenerateCustomerExplanation(
                customer,
                previewTarget,
                previewReward,
                preferredReward);

            results.Add(new CustomerCausalPredictionDto(
                CustomerId: customer.CustomerId,
                CustomerExternalNo: customer.CustomerExternalNo,
                ExpectedOrganicSpend: expectedOrganicSpend,
                PredictedIncrementalUplift: finalUpliftSpend,
                PropensityScore: propensityScore,
                ReactivationProbability: reactivationProb,
                PredictedPersona: predictedPersona,
                RecommendedSector: recommendedSector,
                PreferredRewardType: preferredReward,
                XaiReasonText: explanationText,
                TopFeatureContributions: topFeatures));
        }

        return Result<IReadOnlyList<CustomerCausalPredictionDto>>.Success(results);
    }

    private decimal EstimateClippedPropensityScore(CustomerCausalFeatureView customer)
    {
        decimal rawPropensity = 0.35m + (customer.PortfolioRealSpendQuantile * 0.30m) + (customer.AvailableLimitRatio * 0.15m);
        return Math.Round(Math.Clamp(rawPropensity, _options.PropensityClipMin, _options.PropensityClipMax), 4);
    }

    private static decimal GetSegmentPriorUplift(string segment, decimal organicSpend)
    {
        decimal multiplier = segment switch
        {
            "Private" => 0.25m,
            "Gold" => 0.42m,
            "Silver" => 0.45m,
            _ => 0.38m
        };

        return Math.Round(Math.Max(2500.00m, organicSpend * multiplier), 2);
    }

    private static CausalCustomerInput MapToCausalInput(CustomerCausalFeatureView customer)
    {
        return new CausalCustomerInput
        {
            RealBaselineSpendIndex = (float)customer.RealBaselineSpendIndex,
            AvailableLimitRatio = (float)customer.AvailableLimitRatio,
            MisMonthlyNetProfit = (float)customer.MisMonthlyNetProfit,
            ValidTxCount = customer.ValidTxCount,
            DistinctSectorCount = customer.DistinctSectorCount,
            PortfolioRealSpendQuantile = (float)customer.PortfolioRealSpendQuantile,
            IsGoldOrPrivateSegment = customer.PrestigeSegment is "Gold" or "Private" ? 1.0f : 0.0f
        };
    }

    private static (ITransformer Stage1OrganicModel, ITransformer Stage2UpliftModel) TrainTwoStageRobinsonRLearnerModels(
        MLContext mlContext,
        CausalAiOptions options)
    {
        var rng = new Random(42);
        var stage1Samples = new List<CausalCustomerInput>(240);
        var historicalObservations = new List<(CausalCustomerInput Features, float ActualPostSpend, float TreatmentW, float PropensityE)>(240);

        for (int i = 0; i < 240; i++)
        {
            bool isFreeRider = i % 5 == 0;
            bool isGold = i % 2 == 0;
            float realBaseline = isFreeRider
                ? 55000f + (i * 120f)
                : 8000f + ((i % 25) * 1200f);

            float limitRatio = 0.45f + ((i % 10) * 0.05f);
            float misProfit = isFreeRider ? 7500f : 1200f + ((i % 15) * 220f);
            float txCount = isFreeRider ? 28f : 4f + (i % 12);
            float sectorCount = 1f + (i % 6);
            float quantile = isFreeRider ? 0.92f : Math.Clamp((i % 25) / 25.0f, 0.05f, 0.85f);

            float trueOrganic = realBaseline * 1.02f;
            float trueTreatmentUplift = isFreeRider
                ? realBaseline * 0.02f
                : (realBaseline * 0.44f) + (isGold ? 1800f : 600f);

            float propensityE = Math.Clamp(0.35f + (quantile * 0.35f), (float)options.PropensityClipMin, (float)options.PropensityClipMax);
            float treatmentW = rng.NextDouble() < propensityE ? 1.0f : 0.0f;
            float actualSpendY = trueOrganic + (treatmentW * trueTreatmentUplift);

            var featureRow = new CausalCustomerInput
            {
                RealBaselineSpendIndex = realBaseline,
                AvailableLimitRatio = limitRatio,
                MisMonthlyNetProfit = misProfit,
                ValidTxCount = txCount,
                DistinctSectorCount = sectorCount,
                PortfolioRealSpendQuantile = quantile,
                IsGoldOrPrivateSegment = isGold ? 1.0f : 0.0f,
                Label = trueOrganic,
                Weight = 1.0f
            };

            stage1Samples.Add(featureRow);
            historicalObservations.Add((featureRow, actualSpendY, treatmentW, propensityE));
        }

        string[] featureColumns =
        [
            nameof(CausalCustomerInput.RealBaselineSpendIndex),
            nameof(CausalCustomerInput.AvailableLimitRatio),
            nameof(CausalCustomerInput.MisMonthlyNetProfit),
            nameof(CausalCustomerInput.ValidTxCount),
            nameof(CausalCustomerInput.DistinctSectorCount),
            nameof(CausalCustomerInput.PortfolioRealSpendQuantile),
            nameof(CausalCustomerInput.IsGoldOrPrivateSegment)
        ];

        // Stage 1 Training: Expected Organic Spend m_hat(X)
        IDataView stage1Data = mlContext.Data.LoadFromEnumerable(stage1Samples);
        var stage1Pipeline = mlContext.Transforms.Concatenate("Features", featureColumns)
            .Append(mlContext.Regression.Trainers.LightGbm(
                labelColumnName: nameof(CausalCustomerInput.Label),
                featureColumnName: "Features",
                numberOfLeaves: 15,
                minimumExampleCountPerLeaf: 5,
                learningRate: 0.12,
                numberOfIterations: 50));

        ITransformer stage1Model = stage1Pipeline.Fit(stage1Data);
        using var stage1Predictor = mlContext.Model.CreatePredictionEngine<CausalCustomerInput, RegressionScorePrediction>(stage1Model);

        // Stage 2 Robinson R-Learner: PseudoOutcome = (Y - m_hat(X)) / (W - e_hat(X)), Weight = (W - e_hat(X))^2
        var stage2Samples = new List<CausalCustomerInput>(historicalObservations.Count);
        foreach (var obs in historicalObservations)
        {
            float mHat = stage1Predictor.Predict(obs.Features).Score;
            float yResidual = obs.ActualPostSpend - mHat;
            float wResidual = obs.TreatmentW - obs.PropensityE;

            if (Math.Abs(wResidual) < 0.05f)
            {
                wResidual = wResidual >= 0 ? 0.05f : -0.05f;
            }

            float pseudoOutcome = Math.Clamp(yResidual / wResidual, 0f, 35000f);
            float sampleWeight = wResidual * wResidual;

            stage2Samples.Add(new CausalCustomerInput
            {
                RealBaselineSpendIndex = obs.Features.RealBaselineSpendIndex,
                AvailableLimitRatio = obs.Features.AvailableLimitRatio,
                MisMonthlyNetProfit = obs.Features.MisMonthlyNetProfit,
                ValidTxCount = obs.Features.ValidTxCount,
                DistinctSectorCount = obs.Features.DistinctSectorCount,
                PortfolioRealSpendQuantile = obs.Features.PortfolioRealSpendQuantile,
                IsGoldOrPrivateSegment = obs.Features.IsGoldOrPrivateSegment,
                Label = pseudoOutcome,
                Weight = sampleWeight
            });
        }

        IDataView stage2Data = mlContext.Data.LoadFromEnumerable(stage2Samples);
        var stage2Pipeline = mlContext.Transforms.Concatenate("Features", featureColumns)
            .Append(mlContext.Regression.Trainers.LightGbm(
                labelColumnName: nameof(CausalCustomerInput.Label),
                featureColumnName: "Features",
                exampleWeightColumnName: nameof(CausalCustomerInput.Weight),
                numberOfLeaves: 15,
                minimumExampleCountPerLeaf: 5,
                learningRate: 0.12,
                numberOfIterations: 50));

        ITransformer stage2Model = stage2Pipeline.Fit(stage2Data);
        return (stage1Model, stage2Model);
    }
}
