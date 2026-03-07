using Microsoft.EntityFrameworkCore;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities.Abstractions;

namespace NetworkMapper.Infrastructure.Persistence.Repositories;

internal class Repository<TEntity, TKey> : ReadOnlyRepository<TEntity, TKey>, IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    public Repository(DbContext dbContext) : base(dbContext)
    {
    }

    public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        var createdEntity = await _dbSet.AddAsync(entity, cancellationToken);

        return createdEntity.Entity;
    }

    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        var updatedEntity = _dbSet.Update(entity);

        return Task.FromResult(updatedEntity.Entity);
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken)
    {
        _dbSet.Remove(entity);

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyCollection<TEntity>> AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken)
    {
        var entityList = entities.ToList();

        await _dbSet.AddRangeAsync(entityList, cancellationToken);

        return entityList;
    }
}