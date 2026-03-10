using System.Data;
using Microsoft.Extensions.Logging;
using NetworkMapper.Application.Worker.Parsers.Abstractions;
using NetworkMapper.Application.Worker.Runners.Abstractions;
using NetworkMapper.Application.Worker.Services.Abstractions;
using NetworkMapper.Contracts.Scans;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Application.Worker.Services;

internal sealed class ScansService : IScansService
{
    private readonly IScanRunner _runner;
    private readonly IScanParser _parser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ScansService> _logger;

    public ScansService(
        IScanRunner runner,
        IScanParser parser,
        IUnitOfWork unitOfWork,
        ILogger<ScansService> logger)
    {
        _runner = runner;
        _parser = parser;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task PerformScanAsync(NmapScanDto scanDto, CancellationToken cancellationToken = default)
    {
        var claimed = await _unitOfWork.Scans.ClaimScanAsync(scanDto.Id, cancellationToken);
        if (!claimed)
        {
            return;
        }

        await TryPerformScanAsync(scanDto, cancellationToken);
    }

    private async Task TryPerformScanAsync(
        NmapScanDto scanDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var results = await GetResultsAsync(scanDto, cancellationToken);
            await TryCompleteScanAsync(scanDto.Id, results, cancellationToken);

            _logger.LogInformation("Scan {ScanId} completed successfully.", scanDto.Id);
        }
        catch (Exception ex)
        {
            await HandleScanFailureAsync(scanDto.Id, ex);
        }
    }

    private async Task TryCompleteScanAsync(
        Guid scanId,
        IEnumerable<ScanResult> results,
        CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);

            await _unitOfWork.ScanResults.AddRangeAsync(results, cancellationToken);
            await _unitOfWork.Scans.MarkAsCompletedAsync(scanId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task HandleScanFailureAsync(Guid scanId, Exception ex)
    {
        _logger.LogError(ex, "Scan {ScanId} failed to complete.", scanId);
        await _unitOfWork.Scans.MarkAsFailedAsync(scanId, ex.Message, CancellationToken.None);
    }

    private async Task<IList<ScanResult>> GetResultsAsync(NmapScanDto scanDto,
        CancellationToken cancellationToken)
    {
        var xmlOutput = await _runner.RunScanAsync(scanDto.Target, cancellationToken);
        var scanResults = _parser.Parse(xmlOutput, scanDto.Id);

        return scanResults;
    }
}