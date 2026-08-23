using NetworkMapper.Application.Mappers;
using NetworkMapper.Application.Services.Abstractions;
using NetworkMapper.Application.Services.Models;
using NetworkMapper.Application.Validation;
using NetworkMapper.Contracts.Ports;
using NetworkMapper.Contracts.Scans;
using NetworkMapper.Contracts.Scans.Requests;
using NetworkMapper.Contracts.Scans.Responses;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Domain.Results.Generics;

namespace NetworkMapper.Application.Services;

internal sealed class ScansDiffService : IScansDiffService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationOrchestrator _validationOrchestrator;

    public ScansDiffService(IUnitOfWork unitOfWork, IValidationOrchestrator validationOrchestrator)
    {
        _unitOfWork = unitOfWork;
        _validationOrchestrator = validationOrchestrator;
    }

    public async Task<Result<GetScansDiffResponseDto>> GetDiffAsync(
        GetScansDiffRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationOrchestrator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result<GetScansDiffResponseDto>.ValidationFailure(validationResult.Error);
        }

        var resolution = await ResolveScanPairAsync(request, cancellationToken);
        if (resolution.HasError)
            return Result<GetScansDiffResponseDto>.ValidationFailure(resolution.Error!);

        if (resolution.IsNotFound || resolution.OlderScan is null || resolution.NewerScan is null)
            return Result<GetScansDiffResponseDto>.NotFound();

        return Result<GetScansDiffResponseDto>.Success(
            CalculateDiff(resolution.OlderScan, resolution.NewerScan));
    }

    private async Task<ScanPairResolution> ResolveScanPairAsync(
        GetScansDiffRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!request.From.HasValue && !request.To.HasValue)
            return await GetLatestPairAsync(request.Target, cancellationToken);

        if (request.From.HasValue && !request.To.HasValue)
            return await GetFromToLatestPairAsync(request.Target, request.From.Value, cancellationToken);

        return await GetExplicitPairAsync(request.From!.Value, request.To!.Value, cancellationToken);
    }

    private async Task<ScanPairResolution> GetLatestPairAsync(
        string target,
        CancellationToken cancellationToken)
    {
        var latestScans = await _unitOfWork.Scans
            .GetLatestCompletedScansAsync(target, 2, cancellationToken);

        if (latestScans.Count < 2)
        {
            return new ScanPairResolution(null, null, "Not enough completed scans to perform a diff.");
        }

        return new ScanPairResolution(latestScans[1], latestScans[0], null);
    }

    private async Task<ScanPairResolution> GetFromToLatestPairAsync(
        string target,
        Guid fromId,
        CancellationToken cancellationToken)
    {
        var olderScan = await _unitOfWork.Scans
            .GetScanWithResultsByIdAsync(fromId, cancellationToken);

        var latestScans = await _unitOfWork.Scans
            .GetLatestCompletedScansAsync(target, 2, cancellationToken);

        var latestDifferentScan = latestScans.FirstOrDefault(s => s.Id != fromId);

        if (latestDifferentScan is null)
        {
            return new ScanPairResolution(null, null,
                "Not enough completed scans to compare the requested scan with a newer one.");
        }

        return new ScanPairResolution(olderScan, latestDifferentScan, null);
    }

    private async Task<ScanPairResolution> GetExplicitPairAsync(
        Guid fromId,
        Guid toId,
        CancellationToken cancellationToken)
    {
        var olderScan = await _unitOfWork.Scans.GetScanWithResultsByIdAsync(fromId, cancellationToken);
        var newerScan = await _unitOfWork.Scans.GetScanWithResultsByIdAsync(toId, cancellationToken);

        return new ScanPairResolution(olderScan, newerScan, null);
    }

    private static GetScansDiffResponseDto CalculateDiff(Scan olderScan, Scan newerScan)
    {
        var olderResults = olderScan.Results.ToDictionary(PortKey.From);
        var newerResults = newerScan.Results.ToDictionary(PortKey.From);

        var analysis = AnalyzeNewerResults(olderResults, newerResults);
        var removed = GetRemovedPorts(olderResults, newerResults);

        return new GetScansDiffResponseDto(
            analysis.Added,
            removed,
            analysis.Changed,
            analysis.Unchanged);
    }

    private static PortAnalysisResult AnalyzeNewerResults(
        Dictionary<PortKey, ScanResult> olderResults,
        Dictionary<PortKey, ScanResult> newerResults)
    {
        var added = new List<ScanResultDto>();
        var changed = new List<PortStateChangeDto>();
        var unchanged = new List<ScanResultDto>();

        foreach (var (key, newerResult) in newerResults)
        {
            if (!olderResults.TryGetValue(key, out var olderResult))
            {
                added.Add(newerResult.ToDto());
            }
            else if (!string.Equals(olderResult.State, newerResult.State, StringComparison.OrdinalIgnoreCase))
            {
                changed.Add(new PortStateChangeDto(
                    newerResult.Port,
                    newerResult.Protocol,
                    olderResult.State,
                    newerResult.State));
            }
            else
            {
                unchanged.Add(newerResult.ToDto());
            }
        }

        return new PortAnalysisResult(added, changed, unchanged);
    }

    private static List<ScanResultDto> GetRemovedPorts(
        Dictionary<PortKey, ScanResult> olderResults,
        Dictionary<PortKey, ScanResult> newerResults)
    {
        return olderResults
            .Where(kvp => !newerResults.ContainsKey(kvp.Key))
            .Select(kvp => kvp.Value.ToDto())
            .ToList();
    }
}