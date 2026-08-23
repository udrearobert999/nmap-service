using Vantage.Domain.Entities;

namespace Vantage.Domain.Abstractions;

public interface IOutboxMessageRepository : IRepository<OutboxMessage, Guid>
{
    Task<IList<OutboxMessage>> ClaimScanAsync(int batchSize, CancellationToken cancellationToken = default);
    Task MarkAsCompletedAsync(Guid id, CancellationToken cancellationToken);
    Task MarkAsFailedAsync(Guid id, string errorMessage, CancellationToken cancellationToken);
}