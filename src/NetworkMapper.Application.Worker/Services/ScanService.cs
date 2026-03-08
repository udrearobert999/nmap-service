using Microsoft.Extensions.Logging;
using NetworkMapper.Application.Worker.Parsers.Abstractions;
using NetworkMapper.Application.Worker.Runners.Abstractions;
using NetworkMapper.Application.Worker.Services.Abstractions;
using NetworkMapper.Contracts.Scans;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Application.Worker.Services;

internal sealed class ScanService : IScanService
{
    private readonly IScanRunner _runner;
    private readonly IScanParser _parser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ScanService> _logger;

    public ScanService(
        IScanRunner runner,
        IScanParser parser,
        IUnitOfWork unitOfWork,
        ILogger<ScanService> logger)
    {
        _runner = runner;
        _parser = parser;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task PerformScanAsync(NmapScanDto scanDto, CancellationToken cancellationToken = default)
    {
        var claimed = await _unitOfWork.Scans.TryClaimScanAsync(scanDto.Id, cancellationToken);
        if (!claimed)
        {
            return;
        }

        await TryPerformScanAsync(scanDto, cancellationToken);
    }

    private async Task TryPerformScanAsync(NmapScanDto scanDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var scanInfo = await _unitOfWork.Scans.GetByIdAsync(scanDto.Id, cancellationToken, track: false);
            if (scanInfo is null) return;

            var results = await GetResultsAsync(scanInfo.Target, scanDto.Id, cancellationToken);
            await SaveResultsAsync(results, cancellationToken);
            await CompleteScanAsync(scanDto.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            await FailScanAsync(scanDto.Id, ex);
        }
    }

    private async Task<IList<ScanResult>> GetResultsAsync(string target, Guid scanId,
        CancellationToken cancellationToken)
    {
        var xmlOutput = await _runner.RunScanAsync(target, cancellationToken);
        var scanResults = _parser.Parse(xmlOutput, scanId);

        return scanResults;
    }

    private async Task SaveResultsAsync(IList<ScanResult> results, CancellationToken cancellationToken)
    {
        await _unitOfWork.ScansResults.AddRangeAsync(results, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task CompleteScanAsync(Guid scanId, CancellationToken cancellationToken)
    {
        await _unitOfWork.Scans.MarkAsCompletedAsync(scanId, cancellationToken);
        _logger.LogInformation("Scan {scanId} completed successfully.", scanId);
    }

    private async Task FailScanAsync(Guid scanId, Exception ex)
    {
        _logger.LogError(ex, "Scan {scanId} failed to complete.", scanId);
        await _unitOfWork.Scans.MarkAsFailedAsync(scanId, CancellationToken.None);
    }
}