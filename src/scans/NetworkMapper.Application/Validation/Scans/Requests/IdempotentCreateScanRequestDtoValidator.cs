using FluentValidation;
using NetworkMapper.Application.Validation.Shared;
using NetworkMapper.Contracts.Scans.Requests;
using NetworkMapper.Domain.Abstractions;

namespace NetworkMapper.Application.Validation.Scans.Requests;

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