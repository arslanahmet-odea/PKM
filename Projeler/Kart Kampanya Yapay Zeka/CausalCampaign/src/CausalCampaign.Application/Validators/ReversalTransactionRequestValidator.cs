using CausalCampaign.Domain.DTOs;
using FluentValidation;

namespace CausalCampaign.Application.Validators;

/// <summary>
/// FluentValidation validator for real-time ISO 8583 0400/0420 reversal or refund requests.
/// </summary>
public sealed class ReversalTransactionRequestValidator : AbstractValidator<ReversalTransactionRequestDto>
{
    public ReversalTransactionRequestValidator()
    {
        RuleFor(x => x.ReversalAuthCode)
            .NotEmpty().WithMessage("ReversalAuthCode cannot be empty.")
            .MaximumLength(20);

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("A valid CustomerId (> 0) must be provided.");

        RuleFor(x => x.CampaignId)
            .GreaterThan(0).WithMessage("A valid CampaignId (> 0) must be provided.");

        RuleFor(x => x.OriginalTransactionId)
            .GreaterThan(0L).WithMessage("A valid OriginalTransactionId (> 0) must be provided.");

        RuleFor(x => x.RefundAmount)
            .GreaterThan(0m).WithMessage("RefundAmount must be greater than zero.");
    }
}
