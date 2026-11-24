using CausalCampaign.Application.Validators;
using CausalCampaign.Domain.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CausalCampaign.UnitTests.ApplicationTests;

[TestClass]
public class RequestValidatorsTests
{
    [TestMethod]
    public void AuthorizeTransactionRequestValidator_WhenMccIsNonNumericOrAmountZero_FailsValidation()
    {
        // Arrange
        var validator = new AuthorizeTransactionRequestValidator();
        var invalidRequest = new AuthorizeTransactionRequestDto(
            AuthorizationCode: "",
            CustomerId: 0,
            CampaignId: 1,
            Amount: -50m,
            Currency: "TRY",
            RawMcc_DE18: "54A1",
            RawMerchantName_DE43: "AB",
            TransactionTimestampUtc: DateTime.UtcNow);

        // Act
        var result = validator.Validate(invalidRequest);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Count >= 4);
    }

    [TestMethod]
    public void SelectAlternativeOfferRequestValidator_WithValidIndexTwo_PassesValidation()
    {
        // Arrange
        var validator = new SelectAlternativeOfferRequestValidator();
        var validRequest = new SelectAlternativeOfferRequestDto(AssignmentId: 1, CustomerId: 1001, SelectedOfferIndex: 2);

        // Act
        var result = validator.Validate(validRequest);

        // Assert
        Assert.IsTrue(result.IsValid);
    }
}
