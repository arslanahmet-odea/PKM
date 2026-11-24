using Microsoft.ML.Data;

namespace CausalCampaign.PosEnrichment.Models;

/// <summary>
/// ML.NET Layer 3 multiclass POS merchant classification training and prediction input schema.
/// </summary>
public sealed class MerchantTextInput
{
    [LoadColumn(0)]
    public string MerchantText { get; set; } = string.Empty;

    [LoadColumn(1)]
    public string MccCode { get; set; } = string.Empty;

    [LoadColumn(2)]
    public string SectorLabel { get; set; } = string.Empty;
}

/// <summary>
/// ML.NET Layer 3 POS classification prediction output schema.
/// </summary>
public sealed class MerchantSectorPrediction
{
    [ColumnName("PredictedLabel")]
    public string PredictedSector { get; set; } = string.Empty;

    [ColumnName("Score")]
    public float[] Scores { get; set; } = Array.Empty<float>();
}
