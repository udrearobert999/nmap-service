using Microsoft.Extensions.Logging;
using NetworkMapper.Application.Worker.Parsers.Abstractions;
using NetworkMapper.Application.Worker.Runners.Abstractions;
using NetworkMapper.Application.Worker.Services.Abstractions;
using NetworkMapper.Contracts.Scans;
using NetworkMapper.Domain.Abstractions;

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
        _logger.LogInformation("--> [1] Attempting to claim scan {ScanId}...", scanDto.Id);

        var claimed = await _unitOfWork.Scans.TryClaimScanAsync(scanDto.Id, cancellationToken);
        if (!claimed)
        {
            _logger.LogWarning("--> [X] Scan {ScanId} was already claimed or is not Pending. Exiting.", scanDto.Id);
            return;
        }

        try
        {
            _logger.LogInformation("--> [2] Claim successful. Fetching target...");
            var scanInfo = await _unitOfWork.Scans.GetByIdAsync(scanDto.Id, cancellationToken, track: false);
            if (scanInfo is null) return;

            _logger.LogInformation("--> [3] Starting Nmap runner for {Target}.", scanInfo.Target);
            var xmlOutput = await _runner.RunScanAsync(scanInfo.Target, cancellationToken);
            var scanResults = _parser.Parse(xmlOutput, scanDto.Id);

            _logger.LogInformation("--> [4] Saving Nmap results to database...");
            
            await _unitOfWork.ScansResults.AddRangeAsync(scanResults, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("--> [5] Atomically marking scan as Completed...");
            await _unitOfWork.Scans.MarkAsCompletedAsync(scanDto.Id, cancellationToken);

            _logger.LogInformation("--> [SUCCESS] Worker finished cleanly.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "--> [FAIL] Scan {ScanId} failed with exception.", scanDto.Id);
            await _unitOfWork.Scans.MarkAsFailedAsync(scanDto.Id, CancellationToken.None);
        }
    }
}