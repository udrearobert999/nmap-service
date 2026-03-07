using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Domain.Abstractions;

public interface IScanRepository : IRepository<Scan, Guid>
{
    Task<bool> TryClaimScanAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAsCompletedAsync(Guid id, CancellationToken cancellationToken = default);
}