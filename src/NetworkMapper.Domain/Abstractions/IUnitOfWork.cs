using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Domain.Abstractions;

public interface IUnitOfWork
{
    public IScanRepository Scans { get; set; }
    public IRepository<OutboxMessage, Guid> OutboxMessages { get; set; }
    public IRepository<ScanResult, Guid> ScansResults { get; set; }
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
    public bool IsUniqueConstraintViolation(Exception exception);
}