using CausalCampaign.Domain.DTOs;
using FluentValidation;

namespace CausalCampaign.Application.Validators;

/// <summary>
/// FluentValidation validator for customer selection among 3 equivalent offers (Active Preference Learning).
/// </summary>
public sealed class SelectAlternativeOfferRequestValidator : AbstractValidator<SelectAlternativeOfferRequestDto>
{
    public SelectAlternativeOfferRequestValidator()
    {
        RuleFor(x => x.AssignmentId)
            .GreaterThan(0).WithMessage("A valid AssignmentId (> 0) must be provided.");

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("A valid CustomerId (> 0) must be provided.");

        RuleFor(x => x.SelectedOfferIndex)
            .InclusiveBetween((byte)1, (byte)3)
            .WithMessage("SelectedOfferIndex must be 1, 2, or 3.");
    }
}
