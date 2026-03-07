using System.Linq.Expressions;
using NetworkMapper.Domain.Entities.Abstractions;

namespace NetworkMapper.Domain.Abstractions;

public interface ISpecification<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : struct

{
    public Expression<Func<TEntity, bool>>? Criteria { get; }
    public List<Expression<Func<TEntity, object>>> Includes { get; }
    public Expression<Func<TEntity, object>>? OrderBy { get; }
    public Expression<Func<TEntity, object>>? OrderByDescending { get; }
    public Expression<Func<TEntity, object>>? GroupBy { get; }

    public int? Page { get; }
    public int? PageSize { get; }
    public bool IsPagingEnabled { get; }
    public bool SplitQuery { get; }
    public bool Track { get; }
}