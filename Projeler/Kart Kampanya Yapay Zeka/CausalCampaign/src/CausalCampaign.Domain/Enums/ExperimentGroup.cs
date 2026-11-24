namespace CausalCampaign.Domain.Enums;

/// <summary>
/// Specifies the Stratified Randomized Controlled Trial (Stratified RCT) experiment group assignment.
/// </summary>
public enum ExperimentGroup
{
    /// <summary>Treatment group receiving the campaign offer (90%).</summary>
    Treatment = 1,

    /// <summary>Control hold-out group receiving no offer to measure organic spend and CUPED net lift (10%).</summary>
    Control_Holdout = 2
}
