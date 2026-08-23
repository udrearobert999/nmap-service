using Vantage.Contracts.Scans;

namespace Vantage.Application.Worker.Services.Abstractions;

public interface IScansService
{
    Task PerformScanAsync(NmapScanDto scanDto, CancellationToken cancellationToken);
}