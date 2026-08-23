using Vantage.Application.Services.Models;

namespace Vantage.Application.Services.Abstractions;

public interface IIdentitySyncService
{
    Task<CurrentIdentity> SyncAsync(ClerkIdentity identity, CancellationToken cancellationToken = default);
}
