using NetworkMapper.Application.Services.Models;

namespace NetworkMapper.Application.Services.Abstractions;

public interface IIdentitySyncService
{
    Task<CurrentIdentity> SyncAsync(ClerkIdentity identity, CancellationToken cancellationToken = default);
}
