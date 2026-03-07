using Microsoft.EntityFrameworkCore;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Infrastructure.Persistence.Repositories;

internal class ScanRepository : Repository<Scan, Guid>, IScanRepository
{
    public ScanRepository(DbContext dbContext) : base(dbContext)
    {
    }

    public async Task<bool> TryClaimScanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rowsAffected = await _dbSet
            .Where(s => s.Id == id && s.Status == "Pending")
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "Running"), cancellationToken);

        return rowsAffected > 0;
    }

    public async Task MarkAsFailedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(s => s.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "Failed"), cancellationToken);
    }
    
    public async Task MarkAsCompletedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(s => s.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Status, "Completed")
                .SetProperty(x => x.CompletedAt, DateTime.UtcNow), cancellationToken);
    }
}