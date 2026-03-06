using NetworkMapper.Contracts.Scans.Options;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Application.Specifications;

internal sealed class GetAllScansWithResultsPagedSpec : Specification<Scan, Guid>
{
    public GetAllScansWithResultsPagedSpec(GetAllScansOptionsDto options) :
        base(m => options.Target == null || m.Target.Contains(options.Target))
    {
        ApplyPaging(options.Page, options.PageSize);
        ApplyOrderBy(options.OrderBy, options.OrderDirection);
        DisableTracking();
    }
}