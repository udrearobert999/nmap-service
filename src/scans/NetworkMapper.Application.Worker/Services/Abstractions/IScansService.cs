using NetworkMapper.Contracts.Scans;

namespace NetworkMapper.Application.Worker.Services.Abstractions;

public interface IScansService
{
    Task PerformScanAsync(NmapScanDto scanDto, CancellationToken cancellationToken);
}