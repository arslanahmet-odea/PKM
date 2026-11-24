using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.DTOs;

namespace CausalCampaign.Domain.Interfaces;

/// <summary>
/// Service contract for monitoring inflationary data drift (PSI) and density-based behavioral micro-clustering (HDBSCAN).
/// </summary>
public interface IDriftAndClusteringService
{
    /// <summary>
    /// Calculates the Population Stability Index (PSI) between reference and current spend distributions.
    /// </summary>
    Result<PsiDriftReportDto> CalculateMonthlyPsiDrift(
        int referenceYearMonth,
        int currentYearMonth,
        IReadOnlyList<decimal> referenceNominalSpends,
        IReadOnlyList<decimal> currentNominalSpends,
        IReadOnlyList<decimal> referenceRealSpends,
        IReadOnlyList<decimal> currentRealSpends);
}
