using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Domain.Abstractions;

public interface IOutboxMessageRepository : IRepository<OutboxMessage, Guid>
{
    Task<IList<OutboxMessage>> ClaimScanAsync(int batchSize, CancellationToken cancellationToken = default);
    Task MarkAsCompletedAsync(Guid id, CancellationToken cancellationToken);
    Task MarkAsFailedAsync(Guid id, string errorMessage, CancellationToken cancellationToken);
}