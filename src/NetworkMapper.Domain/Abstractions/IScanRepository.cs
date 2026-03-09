using NetworkMapper.Contracts.Scans.Options;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Domain.Abstractions;

public interface IScanRepository : IRepository<Scan, Guid>
{
    Task<bool> TryClaimScanAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(Guid id, string errorMessage, CancellationToken cancellationToken = default);
    Task MarkAsCompletedAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Scan>> GetScansAsync(GetScansOptionsDto options,
        CancellationToken cancellationToken = default);

    Task<Scan?> GetScanWithResultsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IList<Scan>> GetLatestCompletedScansAsync(string target, int count,
        CancellationToken cancellationToken = default);
}