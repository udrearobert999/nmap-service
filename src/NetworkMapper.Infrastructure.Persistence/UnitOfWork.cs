using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Infrastructure.Persistence.Repositories;
using Npgsql;

namespace NetworkMapper.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly DbContext _dbContext;
    private IDbContextTransaction? _transaction;

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

    public async Task BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("A transaction is already active.");

        _transaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction to commit.");

        try
        {
            await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            return;

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    public bool IsUniqueConstraintViolation(Exception exception)
    {
        return exception is DbUpdateException
        {
            InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }
        };
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}