using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities.Abstractions;

namespace NetworkMapper.Infrastructure.Persistence.Repositories;

internal class ReadOnlyRepository<TEntity, TKey> : IReadOnlyRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    protected readonly DbContext _dbContext;
    protected readonly DbSet<TEntity> _dbSet;

    public ReadOnlyRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<TEntity>();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetByExpressionAsync(
        Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default,
        bool track = false)
    {
        var query = track ? _dbSet.AsQueryable() : _dbSet.AsNoTracking();
        return await query.Where(expression).ToListAsync(cancellationToken);
    }
    
    public async Task<TEntity?> GetByIdAsync(TKey key, CancellationToken cancellationToken = default, bool track = true)
    {
        if (track)
            return await _dbSet.FindAsync([key], cancellationToken);

        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id.Equals(key), cancellationToken);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default,
        bool track = false)
    {
        var query = track ? _dbSet.AsQueryable() : _dbSet.AsNoTracking();
        return await query.FirstOrDefaultAsync(expression, cancellationToken);
    }
    
    public async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().AnyAsync(expression, cancellationToken);
    }
    
    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? expression = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();

        if (expression is not null)
            query = query.Where(expression);

        return await query.CountAsync(cancellationToken);
    }
}