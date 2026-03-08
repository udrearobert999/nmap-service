using NetworkMapper.Contracts.Scans;
using NetworkMapper.Contracts.Scans.Messages;

namespace NetworkMapper.Application.Worker.Mappers;

public static class ScanMapper
{
    public static NmapScanDto ToDto(this ScanRequestMessage scan)
        => new(scan.ScanId, scan.Target);
}