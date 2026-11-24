namespace CausalCampaign.Domain.Enums;

/// <summary>
/// Specifies which layer resolved the POS transaction text (DE-43 and DE-18 MCC).
/// </summary>
public enum PosClassificationLayer : byte
{
    /// <summary>Layer 1: National chain and known brand deterministic dictionary (&lt; 1 ms).</summary>
    Dictionary = 1,

    /// <summary>Layer 2: Payment facilitator (IYZICO*, PARAM*, PAYTR*, SIPAY*) sub-merchant Regex parser.</summary>
    PaymentFacilitatorRegex = 2,

    /// <summary>Layer 3: ML.NET local semantic text classification model.</summary>
    MachineLearning = 3
}
