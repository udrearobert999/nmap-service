using System.Reflection;
using Microsoft.EntityFrameworkCore;
using NetworkMapper.Contracts.Constants;
using NetworkMapper.Contracts.Scans.Options;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Infrastructure.Persistence.Repositories;

internal sealed class ScanRepository : Repository<Scan, Guid>, IScanRepository
{
    public ScanRepository(DbContext dbContext) : base(dbContext)
    {
    }

    public async Task<bool> ClaimScanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rowsAffected = await _dbSet
            .Where(s => s.Id == id && s.Status == Status.Pending)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, Status.Running), cancellationToken);

        return rowsAffected > 0;
    }

    public async Task MarkAsFailedAsync(Guid id, string errorMessage, CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(s => s.Id == id)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, Status.Failed)
                    .SetProperty(x => x.ErrorMessage, errorMessage),
                cancellationToken);
    }

    public async Task MarkAsCompletedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(s => s.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Status, Status.Completed)
                .SetProperty(x => x.CompletedAt, DateTime.UtcNow), cancellationToken);
    }

    public async Task<Scan?> GetScanWithResultsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Results)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Scan>> GetScansAsync(GetScansOptionsDto options,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(s => s.Results)
            .AsNoTracking();

        query = ApplyFiltering(query, options);
        query = ApplySorting(query, options);
        query = ApplyPaging(query, options);

        return await query.ToListAsync(cancellationToken);
    }

    private static IQueryable<Scan> ApplyFiltering(IQueryable<Scan> query, GetScansOptionsDto options)
    {
        if (!string.IsNullOrWhiteSpace(options.Target))
        {
            return query.Where(s => s.Target.Contains(options.Target));
        }

        return query;
    }

    private static IQueryable<Scan> ApplySorting(IQueryable<Scan> query, GetScansOptionsDto options)
    {
        if (string.IsNullOrWhiteSpace(options.OrderBy))
        {
            return query.OrderByDescending(s => s.CreatedAt);
        }

        var propertyInfo = typeof(Scan).GetProperty(
            options.OrderBy,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (propertyInfo == null)
        {
            return query.OrderByDescending(s => s.CreatedAt);
        }

        var isDescending = !string.Equals(options.OrderDirection, OrderDirectionConstants.Ascending,
            StringComparison.OrdinalIgnoreCase);

        if (isDescending)
        {
            return query.OrderByDescending(s => EF.Property<object>(s, propertyInfo.Name));
        }

        return query.OrderBy(s => EF.Property<object>(s, propertyInfo.Name));
    }

    private static IQueryable<Scan> ApplyPaging(IQueryable<Scan> query, GetScansOptionsDto options)
    {
        if (!options.Page.HasValue && !options.PageSize.HasValue)
        {
            return query;
        }

        if (options.Page is null || options.PageSize is null)
            throw new ArgumentException("Paging enabled but has null values.");

        if (options.Page < 1)
            throw new ArgumentException("Page number must be greater than or equal to 1.");

        var skip = (options.Page.Value - 1) * options.PageSize.Value;

        return query.Skip(skip).Take(options.PageSize.Value);
    }

    public async Task<IList<Scan>> GetLatestCompletedScansAsync(string target, int count,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Results)
            .AsNoTracking()
            .Where(s => s.Target == target && s.Status == Status.Completed)
            .OrderByDescending(s => s.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}