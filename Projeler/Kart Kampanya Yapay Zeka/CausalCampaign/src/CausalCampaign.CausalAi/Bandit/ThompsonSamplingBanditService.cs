using System.Collections.Concurrent;
using MathNet.Numerics.Distributions;

namespace CausalCampaign.CausalAi.Bandit;

/// <summary>
/// Thompson Sampling Contextual Multi-Armed Bandit and active customer preference learning (Discrete Choice)
/// service powered by MathNet.Numerics Beta(alpha, beta) distributions.
/// </summary>
public sealed class ThompsonSamplingBanditService
{
    private readonly ConcurrentDictionary<string, (double Alpha, double Beta)> _armPosteriors = new(StringComparer.OrdinalIgnoreCase);
    private readonly Random _rng = new(42);
    private readonly object _rngLock = new();

    public ThompsonSamplingBanditService()
    {
        // Initial Beta(alpha, beta) prior distributions
        _armPosteriors["RewardPoints"] = (12.0, 6.0);
        _armPosteriors["Cashback"] = (12.0, 6.0);
    }

    /// <summary>
    /// Selects a reward type by combining the customer's historical preference with a Thompson Sampling Beta draw.
    /// </summary>
    public string SelectRewardTypeWithBandit(int customerId, string customerHistoricalPreference)
    {
        var pointsArm = _armPosteriors.GetOrAdd("RewardPoints", (10.0, 5.0));
        var cashbackArm = _armPosteriors.GetOrAdd("Cashback", (10.0, 5.0));

        double pointsSample;
        double cashbackSample;

        lock (_rngLock)
        {
            pointsSample = Beta.Sample(_rng, pointsArm.Alpha, pointsArm.Beta);
            cashbackSample = Beta.Sample(_rng, cashbackArm.Alpha, cashbackArm.Beta);
        }

        // Apply contextual boost based on the customer's known historical preference
        if (string.Equals(customerHistoricalPreference, "Cashback", StringComparison.OrdinalIgnoreCase))
        {
            cashbackSample += 0.25;
        }
        else if (string.Equals(customerHistoricalPreference, "RewardPoints", StringComparison.OrdinalIgnoreCase))
        {
            pointsSample += 0.25;
        }

        return cashbackSample > pointsSample ? "Cashback" : "RewardPoints";
    }

    /// <summary>
    /// Updates the Beta(alpha, beta) posterior parameters of the selected reward arm when a customer chooses
    /// one of the 3 equivalent offers or completes their spend goal.
    /// </summary>
    public (double Alpha, double Beta) RecordCustomerChoiceFeedback(string selectedRewardType, bool acceptedOrCompleted)
    {
        string normalizedArm = string.Equals(selectedRewardType, "Cashback", StringComparison.OrdinalIgnoreCase)
            ? "Cashback"
            : "RewardPoints";

        return _armPosteriors.AddOrUpdate(
            normalizedArm,
            _ => acceptedOrCompleted ? (11.0, 5.0) : (10.0, 6.0),
            (_, current) => acceptedOrCompleted
                ? (current.Alpha + 1.0, current.Beta)
                : (current.Alpha, current.Beta + 1.0));
    }
}
