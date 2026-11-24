using CausalCampaign.Domain.DTOs;
using FluentValidation;

namespace CausalCampaign.Application.Validators;

/// <summary>
/// FluentValidation validator for campaign emergency Kill-Switch requests.
/// </summary>
public sealed class TriggerKillSwitchRequestValidator : AbstractValidator<TriggerKillSwitchRequestDto>
{
    public TriggerKillSwitchRequestValidator()
    {
        RuleFor(x => x.CampaignId)
            .GreaterThan(0).WithMessage("A valid CampaignId (> 0) must be provided.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Kill-Switch reason cannot be empty.")
            .Must(r => !string.IsNullOrWhiteSpace(r) && r.Trim().Length >= 5)
            .WithMessage("Kill-Switch reason must be at least 5 non-whitespace characters long.")
            .MaximumLength(300);

        RuleFor(x => x.TriggeredBy)
            .NotEmpty().WithMessage("TriggeredBy user identifier cannot be empty.")
            .MaximumLength(100);
    }
}
