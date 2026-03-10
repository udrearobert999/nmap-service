using System.Data;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Domain.Abstractions;

public interface IUnitOfWork
{
    public IScanRepository Scans { get; set; }
    public IOutboxMessageRepository OutboxMessages { get; set; }
    public IRepository<ScanResult, Guid> ScanResults { get; set; }
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
    public bool IsUniqueConstraintViolation(Exception exception);
    Task BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}