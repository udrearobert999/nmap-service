using FluentValidation;
using NetworkMapper.Contracts.Scans.Requests;
using NetworkMapper.Domain.Abstractions;

namespace NetworkMapper.Application.Validation.Scans.Requests;

public class IdempotentCreateScanRequestDtoValidator : AbstractValidator<IdempotentCreateScanRequestDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public IdempotentCreateScanRequestDtoValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Target)
            .NotEmpty()
            .WithMessage("Target is required!");
        
        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("A valid X-Idempotency-Key header is required.");
    }
}