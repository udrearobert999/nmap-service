using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Application.Specifications;

internal sealed class GetScanByRequestIdSpec : Specification<Scan, Guid>
{
    public GetScanByRequestIdSpec(Guid requestId) : 
        base(s => s.RequestId == requestId)
    {
        DisableTracking();
    }
}