using CausalCampaign.Domain.Entities;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;

namespace CausalCampaign.CausalAi.Recommendation;

/// <summary>
/// Recommends the next-best merchant sector with the highest cross-sell affinity using
/// ML.NET MatrixFactorization collaborative filtering.
/// </summary>
public sealed class MatrixFactorizationSectorRecommender
{
    private static readonly string[] CandidateSectors =
    [
        "E-Commerce & Marketplace",
        "Grocery & Supermarket",
        "Fuel",
        "Apparel & Fashion",
        "Electronics",
        "Restaurant & Cafe"
    ];

    private readonly MLContext _mlContext;
    private readonly ITransformer _recommenderModel;
    private readonly PredictionEngine<SectorAffinityInteraction, SectorAffinityPrediction> _predictionEngine;
    private readonly object _lock = new();

    public MatrixFactorizationSectorRecommender()
    {
        _mlContext = new MLContext(seed: 42);
        _recommenderModel = TrainMatrixFactorizationModel(_mlContext);
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<SectorAffinityInteraction, SectorAffinityPrediction>(_recommenderModel);
    }

    /// <summary>
    /// Recommends the highest-affinity sector based on the customer's behavioral cluster and spending profile.
    /// </summary>
    public string RecommendNextBestSector(CustomerCausalFeatureView customer)
    {
        uint userArchetypeKey = (uint)Math.Clamp(customer.BehavioralClusterId, 1, 6);
        string bestSector = CandidateSectors[0];
        float bestScore = float.MinValue;

        lock (_lock)
        {
            for (uint sIdx = 0; sIdx < CandidateSectors.Length; sIdx++)
            {
                var pred = _predictionEngine.Predict(new SectorAffinityInteraction
                {
                    UserArchetypeId = userArchetypeKey,
                    SectorId = sIdx + 1
                });

                if (pred.Score > bestScore)
                {
                    bestScore = pred.Score;
                    bestSector = CandidateSectors[sIdx];
                }
            }
        }

        return bestSector;
    }

    private static ITransformer TrainMatrixFactorizationModel(MLContext mlContext)
    {
        var interactions = new List<SectorAffinityInteraction>();
        for (uint archetype = 1; archetype <= 6; archetype++)
        {
            for (uint sector = 1; sector <= (uint)CandidateSectors.Length; sector++)
            {
                float affinity = ((archetype + sector) % 3 == 0) ? 4.8f : 2.5f + ((archetype * sector) % 3) * 0.7f;
                interactions.Add(new SectorAffinityInteraction
                {
                    UserArchetypeId = archetype,
                    SectorId = sector,
                    AffinityScore = affinity
                });
            }
        }

        IDataView dataView = mlContext.Data.LoadFromEnumerable(interactions);
        var options = new MatrixFactorizationTrainer.Options
        {
            MatrixColumnIndexColumnName = nameof(SectorAffinityInteraction.UserArchetypeId),
            MatrixRowIndexColumnName = nameof(SectorAffinityInteraction.SectorId),
            LabelColumnName = nameof(SectorAffinityInteraction.AffinityScore),
            NumberOfIterations = 25,
            ApproximationRank = 4,
            Quiet = true
        };

        var pipeline = mlContext.Recommendation().Trainers.MatrixFactorization(options);
        return pipeline.Fit(dataView);
    }

    private sealed class SectorAffinityInteraction
    {
        [KeyType(count: 10)]
        public uint UserArchetypeId { get; set; }

        [KeyType(count: 10)]
        public uint SectorId { get; set; }

        public float AffinityScore { get; set; }
    }

    private sealed class SectorAffinityPrediction
    {
        public float Score { get; set; }
    }
}
