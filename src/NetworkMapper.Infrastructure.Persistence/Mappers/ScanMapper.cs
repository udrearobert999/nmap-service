using NetworkMapper.Contracts.Scans.Messages;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Infrastructure.Persistence.Mappers;

public static class ScanMapper
{
    public static ScanRequestMessage ToMessage(this Scan scan)
        => new(scan.Id, scan.Target);
}