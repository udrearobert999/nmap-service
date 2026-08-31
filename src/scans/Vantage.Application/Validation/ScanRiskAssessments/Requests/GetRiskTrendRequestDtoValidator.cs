using FluentValidation;
using Vantage.Application.Validation.Shared;
using Vantage.Contracts.ScanRiskAssessments.Requests;

namespace Vantage.Application.Validation.ScanRiskAssessments.Requests;

public sealed class GetRiskTrendRequestDtoValidator : AbstractValidator<GetRiskTrendRequestDto>
{
    public GetRiskTrendRequestDtoValidator()
    {
        RuleFor(x => x.Target)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Target is required.")
            .Must(HostValidator.IsValidHost)
            .WithMessage("Target must be a valid IP address or hostname.");
    }
}
