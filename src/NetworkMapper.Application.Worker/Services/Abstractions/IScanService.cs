using NetworkMapper.Contracts.Scans;

namespace NetworkMapper.Application.Worker.Services.Abstractions;

public interface IScanService
{
    Task PerformScanAsync(NmapScanDto scanDto, CancellationToken cancellationToken);
}