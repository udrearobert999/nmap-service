using Microsoft.EntityFrameworkCore;
using NetworkMapper.Contracts.Constants;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Infrastructure.Persistence.Constants;

namespace NetworkMapper.Infrastructure.Persistence.Repositories;

internal sealed class OutboxMessageRepository : Repository<OutboxMessage, Guid>, IOutboxMessageRepository
{
    public OutboxMessageRepository(DbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IList<OutboxMessage>> ClaimScanAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var sql = $"""
                   UPDATE "{TableNamesConstants.OutboxMessages}"
                   SET "Status" = '{Status.Running}'
                   WHERE "Id" IN (
                       SELECT "Id"
                       FROM "{TableNamesConstants.OutboxMessages}"
                       WHERE "Status" = '{Status.Pending}'
                       ORDER BY "CreatedAt"
                       LIMIT {batchSize}
                       FOR UPDATE SKIP LOCKED
                   )
                   RETURNING *;
                   """;

        return await _dbSet
            .FromSqlRaw(sql)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsCompletedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, Status.Completed)
                    .SetProperty(x => x.ProcessedAt, DateTime.UtcNow),
                cancellationToken);
    }

    public async Task MarkAsFailedAsync(Guid id, string errorMessage, CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, Status.Failed)
                    .SetProperty(x => x.ErrorMessage, errorMessage),
                cancellationToken);
    }
}