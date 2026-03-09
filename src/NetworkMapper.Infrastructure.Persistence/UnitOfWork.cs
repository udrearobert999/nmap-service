using Microsoft.EntityFrameworkCore;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Infrastructure.Persistence.Repositories;
using Npgsql;

namespace NetworkMapper.Infrastructure.Persistence;

internal class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _dbContext;
    public IScanRepository Scans { get; set; }
    public IRepository<OutboxMessage, Guid> OutboxMessages { get; set; }
    public IRepository<ScanResult, Guid> ScansResults { get; set; }

    public UnitOfWork(DbContext dbContext)
    {
        _dbContext = dbContext;

        Scans = new ScanRepository(dbContext);
        ScansResults = new Repository<ScanResult, Guid>(dbContext);
        OutboxMessages = new Repository<OutboxMessage, Guid>(dbContext);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    public bool IsUniqueConstraintViolation(Exception exception)
    {
        return exception is DbUpdateException { InnerException: PostgresException { SqlState: "23505" } };
    }
}