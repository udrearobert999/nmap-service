using System.Linq.Expressions;
using NetworkMapper.Domain.Abstractions.Constants;
using NetworkMapper.Domain.Entities.Abstractions;

namespace NetworkMapper.Domain.Abstractions;

public abstract class Specification<TEntity, TKey> : ISpecification<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : struct

{
    public Expression<Func<TEntity, bool>>? Criteria { get; }
    public List<Expression<Func<TEntity, object>>> Includes { get; } = new();
    public Expression<Func<TEntity, object>>? OrderBy { get; private set; }
    public Expression<Func<TEntity, object>>? OrderByDescending { get; private set; }
    public Expression<Func<TEntity, object>>? GroupBy { get; private set; }

    public int? Page { get; private set; }
    public int? PageSize { get; private set; }
    public bool IsPagingEnabled { get; private set; }
    public bool SplitQuery { get; private set; }
    public bool Track { get; private set; } = true;

    protected Specification()
    {
    }

    protected Specification(Expression<Func<TEntity, bool>>? criteria)
    {
        Criteria = criteria;
    }

    protected virtual void AddInclude(Expression<Func<TEntity, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected virtual void ApplyPaging(int? page, int? pageSize)
    {
        Page = page;
        PageSize = pageSize;

        if (page is not null && pageSize is not null)
            IsPagingEnabled = true;
    }

    protected virtual void ApplyOrderBy(Expression<Func<TEntity, object>> orderByExpression, string direction)
    {
        SetOrderByExpression(orderByExpression, direction);
    }

    protected virtual void ApplyOrderBy(string? propertyName, string? direction)
    {
        if (propertyName is null || direction is null)
            return;

        var expression = CreateOrderByExpressionFromPropertyName(propertyName);

        SetOrderByExpression(expression, direction);
    }

    private Expression<Func<TEntity, object>> CreateOrderByExpressionFromPropertyName(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var property = Expression.Property(parameter, propertyName);
        var expression = Expression.Lambda<Func<TEntity, object>>(property, parameter);

        return expression;
    }

    private void SetOrderByExpression(Expression<Func<TEntity, object>> orderByExpression, string direction)
    {
        if (direction == OrderDirectionConstants.Ascending)
            OrderBy = orderByExpression;
        else
            OrderByDescending = orderByExpression;
    }

    protected virtual void ApplyGroupBy(Expression<Func<TEntity, object>> groupByExpression)
    {
        GroupBy = groupByExpression;
    }

    public void EnableSplitQuery()
    {
        SplitQuery = true;
    }

    public void DisableTracking()
    {
        Track = false;
    }
}