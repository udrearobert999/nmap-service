using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Domain.Abstractions;

public interface IUnitOfWork
{
    public IRepository<Scan, Guid> Scans { get; set; }
    public IRepository<IdempotentRequest, Guid> IdempotentRequests { get; set; }
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}