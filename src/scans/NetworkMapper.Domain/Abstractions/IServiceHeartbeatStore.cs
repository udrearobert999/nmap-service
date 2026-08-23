using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Domain.Abstractions;

public interface IServiceHeartbeatStore
{
    Task UpsertAsync(string serviceName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceHeartbeat>> GetAllAsync(CancellationToken cancellationToken = default);
}
