using Microsoft.EntityFrameworkCore;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities.Abstractions;

namespace NetworkMapper.Infrastructure.Persistence.Specifications;

internal class SpecificationQueryBuilder<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery,
        ISpecification<TEntity, TKey> specification)
    {
        var query = inputQuery;

        if (!specification.Track)
        {
            query = query.AsNoTracking();
        }

        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        query = specification.Includes.Aggregate(query,
            (current, include) => current.Include(include));

        if (specification.SplitQuery)
        {
            query = query.AsSplitQuery();
        }

        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        if (specification.GroupBy != null)
        {
            query = query.GroupBy(specification.GroupBy).SelectMany(x => x);
        }

        if (specification.IsPagingEnabled)
        {
            if (specification.Page is null || specification.PageSize is null)
                throw new ArgumentException("Paging enabled but has null values");

            if (specification.Page < 1)
                throw new ArgumentException("Page number must be greater than or equal 1!");

            var skip = (specification.Page - 1) * specification.PageSize;
            var take = specification.PageSize;

            query = query
                .Skip((int) skip)
                .Take((int) take);
        }

        return query;
    }
}