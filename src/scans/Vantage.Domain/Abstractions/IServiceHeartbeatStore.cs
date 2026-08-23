using Vantage.Domain.Entities;

namespace Vantage.Domain.Abstractions;

public interface IServiceHeartbeatStore
{
    Task UpsertAsync(string serviceName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceHeartbeat>> GetAllAsync(CancellationToken cancellationToken = default);
}
