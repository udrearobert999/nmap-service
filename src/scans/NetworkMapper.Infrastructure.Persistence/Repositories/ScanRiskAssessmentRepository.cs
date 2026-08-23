using System.Reflection;
using Microsoft.EntityFrameworkCore;
using NetworkMapper.Contracts.Constants;
using NetworkMapper.Contracts.ScanRiskAssessments.Options;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Infrastructure.Persistence.Repositories;

internal sealed class ScanRiskAssessmentRepository : Repository<ScanRiskAssessment, Guid>, IScanRiskAssessmentRepository
{
    public ScanRiskAssessmentRepository(DbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<ScanRiskAssessment>> GetPagedAsync(
        GetScanRiskAssessmentsOptionsDto options,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(r => r.CreatedByUser)
            .AsNoTracking();

        query = ApplyFiltering(query, options);
        query = ApplySorting(query, options);
        query = ApplyPaging(query, options);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<ScanRiskAssessment?> GetLatestByScanAsync(Guid scanId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Findings)
            .Include(r => r.CreatedByUser)
            .AsNoTracking()
            .Where(r => r.ScanId == scanId)
            .OrderByDescending(r => r.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ScanRiskAssessment?> GetWithFindingsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Findings)
            .Include(r => r.CreatedByUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    private static IQueryable<ScanRiskAssessment> ApplyFiltering(
        IQueryable<ScanRiskAssessment> query,
        GetScanRiskAssessmentsOptionsDto options)
    {
        if (!string.IsNullOrWhiteSpace(options.Status))
        {
            query = query.Where(r => r.Status == options.Status);
        }

        if (options.ScanId.HasValue)
        {
            query = query.Where(r => r.ScanId == options.ScanId.Value);
        }

        return query;
    }

    private static IQueryable<ScanRiskAssessment> ApplySorting(
        IQueryable<ScanRiskAssessment> query,
        GetScanRiskAssessmentsOptionsDto options)
    {
        if (string.IsNullOrWhiteSpace(options.OrderBy))
        {
            return query.OrderByDescending(r => r.RequestedAt);
        }

        var propertyInfo = typeof(ScanRiskAssessment).GetProperty(
            options.OrderBy,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (propertyInfo == null)
        {
            return query.OrderByDescending(r => r.RequestedAt);
        }

        var isDescending = !string.Equals(options.OrderDirection, OrderDirectionConstants.Ascending,
            StringComparison.OrdinalIgnoreCase);

        if (isDescending)
        {
            return query.OrderByDescending(r => EF.Property<object>(r, propertyInfo.Name));
        }

        return query.OrderBy(r => EF.Property<object>(r, propertyInfo.Name));
    }

    private static IQueryable<ScanRiskAssessment> ApplyPaging(
        IQueryable<ScanRiskAssessment> query,
        GetScanRiskAssessmentsOptionsDto options)
    {
        if (!options.PageNumber.HasValue && !options.PageSize.HasValue)
        {
            return query;
        }

        if (options.PageNumber is null || options.PageSize is null)
            throw new ArgumentException("Paging enabled but has null values.");

        if (options.PageNumber < 1)
            throw new ArgumentException("PageNumber number must be greater than or equal to 1.");

        var skip = (options.PageNumber.Value - 1) * options.PageSize.Value;

        return query.Skip(skip).Take(options.PageSize.Value);
    }
}
