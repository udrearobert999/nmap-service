using NetworkMapper.Domain.Entities.Abstractions;

namespace NetworkMapper.Domain.Abstractions;

public interface IRepository<TEntity, TKey> : IReadOnlyRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    public Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
}