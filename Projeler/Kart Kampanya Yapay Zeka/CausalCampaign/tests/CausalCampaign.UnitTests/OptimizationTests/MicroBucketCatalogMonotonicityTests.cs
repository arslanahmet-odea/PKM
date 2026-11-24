using CausalCampaign.Optimization.Buckets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.OptimizationTests;

[TestClass]
public class MicroBucketCatalogMonotonicityTests
{
    [TestMethod]
    public void GetAllMicroBuckets_ReturnsTwentyStandardBucketsWithZeroMonotonicityViolations()
    {
        // Arrange
        var catalog = new MicroBucketCatalogService();

        // Act
        var buckets = catalog.GetAllMicroBuckets();
        var pairs = buckets.Select(b => (b.BaseTargetAmount, b.BaseRewardAmount)).ToList();
        int violations = MicroBucketCatalogService.CountMonotonicityViolations(pairs);

        // Assert
        Assert.AreEqual(20, buckets.Count);
        Assert.AreEqual(0, violations, "KPI-8: There must be zero monotonicity violations (Target_a > Target_b => Reward_a >= Reward_b) in the 20 Micro-Bucket catalog.");
    }
}
