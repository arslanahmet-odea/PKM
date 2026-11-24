namespace CausalCampaign.Domain.Entities;

/// <summary>
/// Entity representing the INFLATION_INDEX_MONTHLY table for sectoral monthly CPI and real spend deflation.
/// </summary>
public class InflationIndexMonthly
{
    public int InflationIndexId { get; set; }
    public int YearMonth { get; set; }
    public string SectorName { get; set; } = "General";
    public decimal CpiIndexValue { get; set; } = 100.0000m;
    public decimal DeflatorMultiplier { get; set; } = 1.000000m;

    /// <summary>
    /// Converts a nominal TRY spend amount from the given month into current real purchasing power (real TRY).
    /// </summary>
    public decimal ConvertNominalToRealSpend(decimal nominalAmount)
    {
        if (nominalAmount <= 0m || DeflatorMultiplier <= 0m)
        {
            return 0m;
        }

        return Math.Round(nominalAmount * DeflatorMultiplier, 2);
    }
}
