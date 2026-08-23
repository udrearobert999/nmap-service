using FluentValidation;
using Vantage.Application.Validation.Shared;
using Vantage.Contracts.Scans.Requests;
using Vantage.Domain.Abstractions;

namespace Vantage.Application.Validation.Scans.Requests;

public class IdempotentCreateScanRequestDtoValidator : AbstractValidator<IdempotentCreateScanRequestDto>
{
    public IdempotentCreateScanRequestDtoValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Target)
            .NotEmpty()
            .WithMessage("Target is required!")
            .Must(HostValidator.IsValidHost)
            .WithMessage("Target must be a valid IP address or hostname.");

        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("A valid X-Idempotency-Key header is required.");
    }
}