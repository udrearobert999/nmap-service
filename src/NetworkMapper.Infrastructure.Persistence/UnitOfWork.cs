using Microsoft.EntityFrameworkCore;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Infrastructure.Persistence.Repositories;

namespace NetworkMapper.Infrastructure.Persistence;

internal class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _dbContext;
    public IRepository<Scan, Guid> Scans { get; set; }
    public IRepository<IdempotentRequest, Guid> IdempotentRequests { get; set; }

    public UnitOfWork(DbContext dbContext)
    {
        _dbContext = dbContext;

        Scans = new Repository<Scan, Guid>(dbContext);
        IdempotentRequests = new Repository<IdempotentRequest, Guid>(dbContext);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}