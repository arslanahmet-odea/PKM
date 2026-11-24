using CausalCampaign.CausalAi.Configuration;
using CausalCampaign.Domain.Common;
using CausalCampaign.Domain.DTOs;
using CausalCampaign.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace CausalCampaign.CausalAi.Analytics;

/// <summary>
/// KPI-7 (Monthly PSI &lt; 0.15): Detects nominal spending covariate drift in high-inflation environments
/// and verifies the stability of inflation-adjusted real spend distributions.
/// </summary>
public sealed class InflationPsiDriftMonitor : IDriftAndClusteringService
{
    private readonly CausalAiOptions _options;

    public InflationPsiDriftMonitor(IOptions<CausalAiOptions>? options = null)
    {
        _options = options?.Value ?? new CausalAiOptions();
    }

    /// <summary>
    /// Calculates the Population Stability Index (PSI) between reference and current spend distributions
    /// for both nominal TRY and inflation-deflated real TRY.
    /// </summary>
    public Result<PsiDriftReportDto> CalculateMonthlyPsiDrift(
        int referenceYearMonth,
        int currentYearMonth,
        IReadOnlyList<decimal> referenceNominalSpends,
        IReadOnlyList<decimal> currentNominalSpends,
        IReadOnlyList<decimal> referenceRealSpends,
        IReadOnlyList<decimal> currentRealSpends)
    {
        if (referenceNominalSpends is null || referenceNominalSpends.Count < 4 ||
            currentNominalSpends is null || currentNominalSpends.Count < 4 ||
            referenceRealSpends is null || referenceRealSpends.Count < 4 ||
            currentRealSpends is null || currentRealSpends.Count < 4)
        {
            return Result<PsiDriftReportDto>.Failure(DomainError.Validation(
                "INSUFFICIENT_PSI_SAMPLE_SIZE",
                "At least 4 observations in both reference and current periods are required for PSI calculation."));
        }

        decimal nominalPsi = ComputePsiAcrossBins(referenceNominalSpends, currentNominalSpends, binCount: 5);
        decimal realPsi = ComputePsiAcrossBins(referenceRealSpends, currentRealSpends, binCount: 5);
        bool isSafe = realPsi < _options.PsiAlertThreshold;

        string summary = isSafe
            ? $"Post-deflation real PSI ({realPsi:F4}) is within the safe threshold ({_options.PsiAlertThreshold:F2}) (Nominal PSI: {nominalPsi:F4})."
            : $"ALERT: Real PSI ({realPsi:F4}) exceeded the critical drift threshold ({_options.PsiAlertThreshold:F2}); R-Learner retraining is required.";

        return Result<PsiDriftReportDto>.Success(new PsiDriftReportDto(
            ReferenceYearMonth: referenceYearMonth,
            CurrentYearMonth: currentYearMonth,
            NominalSpendPsi: nominalPsi,
            InflationAdjustedRealSpendPsi: realPsi,
            IsDriftWithinSafeThreshold: isSafe,
            DiagnosticSummary: summary));
    }

    /// <summary>
    /// PSI = Sum( (ActualRatio_b - ExpectedRatio_b) * ln(ActualRatio_b / ExpectedRatio_b) )
    /// </summary>
    private static decimal ComputePsiAcrossBins(
        IReadOnlyList<decimal> expectedSample,
        IReadOnlyList<decimal> actualSample,
        int binCount)
    {
        var sortedExpected = expectedSample.OrderBy(x => x).ToArray();
        decimal minVal = sortedExpected[0];
        decimal maxVal = sortedExpected[^1];

        // Handle zero-variance (constant) reference distributions cleanly
        if (minVal == maxVal)
        {
            bool actualAllMatch = actualSample.All(v => v == minVal);
            return actualAllMatch ? 0.0000m : 1.0000m;
        }

        int effectiveBinCount = Math.Min(binCount, Math.Max(2, sortedExpected.Length));
        var rawCutPoints = new List<decimal>(effectiveBinCount - 1);

        for (int b = 1; b < effectiveBinCount; b++)
        {
            int splitIdx = (int)Math.Clamp(Math.Round((double)b / effectiveBinCount * sortedExpected.Length), 1, sortedExpected.Length - 1);
            decimal cut = (sortedExpected[splitIdx - 1] + sortedExpected[splitIdx]) / 2.0m;
            if (rawCutPoints.Count == 0 || cut > rawCutPoints[^1])
            {
                rawCutPoints.Add(cut);
            }
        }

        int finalBinCount = rawCutPoints.Count + 1;
        const double epsilon = 0.0001;
        double[] expRatios = new double[finalBinCount];
        double[] actRatios = new double[finalBinCount];
        double expSum = 0.0;
        double actSum = 0.0;

        for (int b = 0; b < finalBinCount; b++)
        {
            decimal lower = b == 0 ? decimal.MinValue : rawCutPoints[b - 1];
            decimal upper = b == finalBinCount - 1 ? decimal.MaxValue : rawCutPoints[b];

            int expCount = expectedSample.Count(v => (b == 0 ? v >= lower : v > lower) && v <= upper);
            int actCount = actualSample.Count(v => (b == 0 ? v >= lower : v > lower) && v <= upper);

            expRatios[b] = Math.Max(epsilon, (double)expCount / expectedSample.Count);
            actRatios[b] = Math.Max(epsilon, (double)actCount / actualSample.Count);
            expSum += expRatios[b];
            actSum += actRatios[b];
        }

        double psiSum = 0.0;
        for (int b = 0; b < finalBinCount; b++)
        {
            double expNorm = expRatios[b] / expSum;
            double actNorm = actRatios[b] / actSum;
            psiSum += (actNorm - expNorm) * Math.Log(actNorm / expNorm);
        }

        return Math.Round((decimal)Math.Max(0.0, psiSum), 4);
    }
}
