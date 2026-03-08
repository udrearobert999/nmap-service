using System.Linq.Expressions;
using NetworkMapper.Domain.Entities.Abstractions;

namespace NetworkMapper.Domain.Abstractions;

public interface IReadOnlyRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<TEntity>> GetByExpressionAsync(
        Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default,
        bool track = false);

    Task<TEntity?> GetByIdAsync(TKey key, CancellationToken cancellationToken = default, bool track = true);

    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default,
        bool track = false);

    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? expression = null,
        CancellationToken cancellationToken = default);
}