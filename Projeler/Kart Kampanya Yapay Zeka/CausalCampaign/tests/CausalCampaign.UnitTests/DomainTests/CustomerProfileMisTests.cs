using CausalCampaign.Domain.Constants;
using CausalCampaign.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.DomainTests;

[TestClass]
public class CustomerProfileMisTests
{
    [TestMethod]
    public void GetMaxAllowedTargetAmount_ReturnsEightyPercentOfAvailableCreditLimit()
    {
        // Arrange
        var customer = new CustomerProfileMis
        {
            CustomerId = 1006,
            CustomerExternalNo = "CUST-1006",
            TotalCreditLimit = 30000.00m,
            AvailableCreditLimit = 4500.00m
        };

        // Act
        decimal maxAllowedTarget = customer.GetMaxAllowedTargetAmount();

        // Assert
        Assert.AreEqual(3600.00m, maxAllowedTarget);
    }

    [TestMethod]
    public void CheckCampaignEligibility_WhenNoConsentOrBelowMisThreshold_ReturnsFailure()
    {
        // Arrange
        var noConsentCustomer = new CustomerProfileMis
        {
            CustomerExternalNo = "CUST-2001",
            HasMarketingConsent = false,
            MisMonthlyNetProfit = 1500.00m,
            AvailableCreditLimit = 50000.00m
        };
        var negativeMisCustomer = new CustomerProfileMis
        {
            CustomerExternalNo = "CUST-2002",
            HasMarketingConsent = true,
            MisMonthlyNetProfit = -850.00m,
            AvailableCreditLimit = 50000.00m
        };

        // Act
        var res1 = noConsentCustomer.CheckCampaignEligibility(-250.00m);
        var res2 = negativeMisCustomer.CheckCampaignEligibility(-250.00m);

        // Assert
        Assert.IsTrue(res1.IsFailure);
        Assert.AreEqual("CUSTOMER_NO_MARKETING_CONSENT", res1.Error.Code);

        Assert.IsTrue(res2.IsFailure);
        Assert.AreEqual("CUSTOMER_BELOW_MIS_THRESHOLD", res2.Error.Code);
    }

    [TestMethod]
    [DataRow("6011", true)]
    [DataRow("6051", true)]
    [DataRow("7995", true)]
    [DataRow("4829", true)]
    [DataRow("5411", false)]
    [DataRow("5399", false)]
    public void AntiGamingConstants_IsExcludedMcc_CorrectlyClassifiesCashLikeAndValidMccs(string mcc, bool expectedExcluded)
    {
        // Act
        bool actual = AntiGamingConstants.IsExcludedMcc(mcc);

        // Assert
        Assert.AreEqual(expectedExcluded, actual);
    }
}
