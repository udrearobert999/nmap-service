using FluentValidation;
using NetworkMapper.Application.Validation.Shared;
using NetworkMapper.Contracts.Constants;
using NetworkMapper.Contracts.Scans.Requests;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Application.Validation.Scans.Requests;

public sealed class GetScansDiffRequestDtoValidator : AbstractValidator<GetScansDiffRequestDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetScansDiffRequestDtoValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Target)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Target is required.")
            .Must(HostValidator.IsValidHost)
            .WithMessage("Target must be a valid IP address or hostname.")
            .MustAsync(TargetHasCompletedScansAsync)
            .WithMessage("The provided target does not have any completed scans in the database.");

        RuleFor(x => x)
            .Must(HasValidFromToCombination)
            .WithMessage("'To' cannot be provided without 'From'.");

        When(x => x.From.HasValue, () =>
        {
            RuleFor(x => x.From)
                .Cascade(CascadeMode.Stop)
                .MustAsync(ScanExistsAsync)
                .WithMessage("'From' scan was not found.")
                .MustAsync(ScanIsCompletedAsync)
                .WithMessage("'From' scan is not completed.")
                .MustAsync(BelongsToTargetAsync)
                .WithMessage("'From' scan does not belong to the provided target.");
        });

        When(x => x.To.HasValue, () =>
        {
            RuleFor(x => x.To)
                .Cascade(CascadeMode.Stop)
                .MustAsync(ScanExistsAsync)
                .WithMessage("'To' scan was not found.")
                .MustAsync(ScanIsCompletedAsync)
                .WithMessage("'To' scan is not completed.")
                .MustAsync(BelongsToTargetAsync)
                .WithMessage("'To' scan does not belong to the provided target.");
        });

        When(x => x.From.HasValue && x.To.HasValue, () =>
        {
            RuleFor(x => x)
                .MustAsync(FromIsBeforeToAsync)
                .WithMessage("'From' scan must be completed before 'To' scan.");
        });
    }

    private async Task<bool> TargetHasCompletedScansAsync(
        string target,
        CancellationToken cancellationToken)
    {
        var scans = await _unitOfWork.Scans.GetLatestCompletedScansAsync(target, 1, cancellationToken);

        return scans.Count > 0;
    }

    private static bool HasValidFromToCombination(GetScansDiffRequestDto request) =>
        !request.To.HasValue || request.From.HasValue;

    private async Task<bool> ScanExistsAsync(
        Guid? scanId,
        CancellationToken cancellationToken)
    {
        var scan = await GetScanAsync(scanId, cancellationToken);
        return scan is not null;
    }

    private async Task<bool> ScanIsCompletedAsync(
        Guid? scanId,
        CancellationToken cancellationToken)
    {
        var scan = await GetScanAsync(scanId, cancellationToken);
        return IsCompleted(scan);
    }

    private Task<bool> BelongsToTargetAsync(
        GetScansDiffRequestDto request,
        Guid? scanId,
        CancellationToken cancellationToken) =>
        ScanBelongsToTargetAsync(scanId, request.Target, cancellationToken);

    private async Task<bool> ScanBelongsToTargetAsync(
        Guid? scanId,
        string target,
        CancellationToken cancellationToken)
    {
        var scan = await GetScanAsync(scanId, cancellationToken);
        return BelongsToTarget(scan, target);
    }

    private async Task<bool> FromIsBeforeToAsync(
        GetScansDiffRequestDto request,
        CancellationToken cancellationToken)
    {
        var fromScan = await GetScanAsync(request.From, cancellationToken);
        var toScan = await GetScanAsync(request.To, cancellationToken);

        if (!IsCompleted(fromScan) || !IsCompleted(toScan))
            return true;

        return fromScan!.CompletedAt!.Value <= toScan!.CompletedAt!.Value;
    }

    private async Task<Scan?> GetScanAsync(
        Guid? scanId,
        CancellationToken cancellationToken)
    {
        if (!scanId.HasValue)
            return null;

        return await _unitOfWork.Scans.GetScanWithResultsByIdAsync(scanId.Value, cancellationToken);
    }

    private static bool IsCompleted(Scan? scan) =>
        scan is not null &&
        string.Equals(scan.Status, Status.Completed, StringComparison.OrdinalIgnoreCase) &&
        scan.CompletedAt.HasValue;

    private static bool BelongsToTarget(Scan? scan, string target) =>
        scan is not null &&
        string.Equals(scan.Target, target, StringComparison.OrdinalIgnoreCase);
}