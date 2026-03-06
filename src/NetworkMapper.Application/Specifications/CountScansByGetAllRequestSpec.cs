using NetworkMapper.Contracts.Scans.Options;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Application.Specifications;

internal sealed class CountScansByGetAllRequestSpec : Specification<Scan, Guid>
{
    public CountScansByGetAllRequestSpec(GetAllScansOptionsDto options) :
        base(m => options.Target == null || m.Target.Contains(options.Target))
    {
        DisableTracking();
    }
}