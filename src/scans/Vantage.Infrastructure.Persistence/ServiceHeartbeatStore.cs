using Microsoft.EntityFrameworkCore;
using Vantage.Domain.Abstractions;
using Vantage.Domain.Entities;
using Vantage.Infrastructure.Persistence.Constants;

namespace Vantage.Infrastructure.Persistence;

internal sealed class ServiceHeartbeatStore : IServiceHeartbeatStore
{
    private readonly DbContext _dbContext;

    public ServiceHeartbeatStore(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task UpsertAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        var sql =
            "INSERT INTO \"" + TableNamesConstants.ServiceHeartbeats + "\" (\"ServiceName\", \"LastSeenAt\") " +
            "VALUES ({0}, now()) " +
            "ON CONFLICT (\"ServiceName\") DO UPDATE SET \"LastSeenAt\" = now();";

        await _dbContext.Database.ExecuteSqlRawAsync(sql, [serviceName], cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceHeartbeat>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ServiceHeartbeat>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
