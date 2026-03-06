using System.Linq.Expressions;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Domain.Entities.Abstractions;

namespace NetworkMapper.Domain.Abstractions;

public interface IReadOnlyRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    public Task<IEnumerable<TEntity>> GetAll(CancellationToken cancellationToken = default);
    public Task<TEntity?> GetByIdAsync(TKey key,CancellationToken cancellationToken = default, bool track = true);

    public Task<IEnumerable<TEntity>> GetBySpecAsync(ISpecification<TEntity, TKey> spec,
        CancellationToken cancellationToken = default);

    public Task<TEntity?> GetSingleOrDefaultBySpecAsync(ISpecification<TEntity, TKey> spec,
        CancellationToken cancellationToken = default);

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default);

    public Task<bool> ExistsBySpecAsync(ISpecification<TEntity, TKey> spec,
        CancellationToken cancellationToken = default);

    public Task<int> CountAsync(Expression<Func<TEntity, bool>>? expression = null,
        CancellationToken cancellationToken = default);

    public Task<int> CountBySpecAsync(ISpecification<TEntity, TKey> spec,
        CancellationToken cancellationToken = default);
}