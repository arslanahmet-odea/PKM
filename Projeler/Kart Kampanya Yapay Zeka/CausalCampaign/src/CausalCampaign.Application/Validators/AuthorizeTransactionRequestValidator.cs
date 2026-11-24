using CausalCampaign.Domain.DTOs;
using FluentValidation;

namespace CausalCampaign.Application.Validators;

/// <summary>
/// FluentValidation validator for real-time ISO 8583 0100/0200 card authorization requests.
/// </summary>
public sealed class AuthorizeTransactionRequestValidator : AbstractValidator<AuthorizeTransactionRequestDto>
{
    public AuthorizeTransactionRequestValidator()
    {
        RuleFor(x => x.AuthorizationCode)
            .NotEmpty().WithMessage("AuthorizationCode cannot be empty.")
            .MaximumLength(20).WithMessage("AuthorizationCode cannot exceed 20 characters.");

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("A valid CustomerId (> 0) must be provided.");

        RuleFor(x => x.CampaignId)
            .GreaterThan(0).WithMessage("A valid CampaignId (> 0) must be provided.");

        RuleFor(x => x.Amount)
            .GreaterThan(0m).WithMessage("Transaction amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency cannot be empty.")
            .Must(c => string.Equals(c?.Trim(), "TRY", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Only TRY currency is supported for campaign progress tracking without FX conversion.");

        RuleFor(x => x.RawMcc_DE18)
            .NotEmpty().WithMessage("ISO 8583 DE-18 MCC code cannot be empty.")
            .Matches(@"^\d{4}$").WithMessage("ISO 8583 DE-18 MCC code must be a 4-digit numeric string.");

        RuleFor(x => x.RawMerchantName_DE43)
            .NotEmpty().WithMessage("ISO 8583 DE-43 merchant name cannot be empty.")
            .MinimumLength(3).WithMessage("Merchant name must be at least 3 characters long.")
            .MaximumLength(250);
    }
}
