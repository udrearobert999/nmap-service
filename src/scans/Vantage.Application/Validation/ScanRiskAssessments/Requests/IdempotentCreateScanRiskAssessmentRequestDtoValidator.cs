using FluentValidation;
using Vantage.Contracts.Constants;
using Vantage.Contracts.ScanRiskAssessments.Requests;
using Vantage.Domain.Abstractions;
using Vantage.Domain.Entities;

namespace Vantage.Application.Validation.ScanRiskAssessments.Requests;

public sealed class IdempotentCreateScanRiskAssessmentRequestDtoValidator
    : AbstractValidator<IdempotentCreateScanRiskAssessmentRequestDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private Scan? _scan;
    private bool _scanLoaded;

    public IdempotentCreateScanRiskAssessmentRequestDtoValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("A valid X-Idempotency-Key header is required.");

        RuleFor(x => x.ScanId)
            .Cascade(CascadeMode.Stop)
            .MustAsync(ScanExistsAsync)
            .WithMessage("Scan was not found.")
            .MustAsync(ScanIsCompletedAsync)
            .WithMessage("Scan must be in a completed state before a risk assessment can be requested.")
            .MustAsync(ScanHasResultsAsync)
            .WithMessage("Scan has no results to assess.");
    }

    private async Task<bool> ScanExistsAsync(Guid scanId, CancellationToken cancellationToken)
    {
        var scan = await GetScanAsync(scanId, cancellationToken);
        return scan is not null;
    }

    private async Task<bool> ScanIsCompletedAsync(Guid scanId, CancellationToken cancellationToken)
    {
        var scan = await GetScanAsync(scanId, cancellationToken);
        return scan is not null && string.Equals(scan.Status, Status.Completed, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<bool> ScanHasResultsAsync(Guid scanId, CancellationToken cancellationToken)
    {
        var scan = await GetScanAsync(scanId, cancellationToken);
        return scan is not null && scan.Results.Count > 0;
    }

    private async Task<Scan?> GetScanAsync(Guid scanId, CancellationToken cancellationToken)
    {
        if (_scanLoaded)
            return _scan;

        _scan = await _unitOfWork.Scans.GetScanWithResultsByIdAsync(scanId, cancellationToken);
        _scanLoaded = true;

        return _scan;
    }
}
