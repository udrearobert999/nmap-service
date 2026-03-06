using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities.Abstractions;
using NetworkMapper.Infrastructure.Persistence.Specifications;

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

    public async Task<IEnumerable<TEntity>> GetAll(CancellationToken cancellationToken)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> GetByIdAsync(TKey key, CancellationToken cancellationToken, bool track = true)
    {
        if (track)
            return await _dbSet.FindAsync(key, cancellationToken);

        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id.Equals(key), cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetBySpecAsync(ISpecification<TEntity, TKey> spec,
        CancellationToken cancellationToken)
    {
        return await ApplySpecification(spec).ToListAsync(cancellationToken);
    }

    public Task<TEntity?> GetSingleOrDefaultBySpecAsync(ISpecification<TEntity, TKey> spec,
        CancellationToken cancellationToken)
    {
        return ApplySpecification(spec).SingleOrDefaultAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken)
    {
        return _dbSet.AsNoTracking().AnyAsync(expression, cancellationToken);
    }

    public Task<bool> ExistsBySpecAsync(ISpecification<TEntity, TKey> spec,
        CancellationToken cancellationToken)
    {
        return ApplySpecification(spec).AnyAsync(cancellationToken);
    }

    public Task<int> CountAsync(Expression<Func<TEntity, bool>>? expression,
        CancellationToken cancellationToken)
    {
        if (expression is null)
            return _dbSet.AsNoTracking().CountAsync(cancellationToken);

        return _dbSet.AsNoTracking().CountAsync(expression, cancellationToken);
    }

    public Task<int> CountBySpecAsync(ISpecification<TEntity, TKey> spec, CancellationToken cancellationToken)
    {
        return ApplySpecification(spec).CountAsync(cancellationToken);
    }

    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity, TKey> spec)
    {
        return SpecificationQueryBuilder<TEntity, TKey>.GetQuery(_dbSet.AsQueryable(), spec);
    }
}