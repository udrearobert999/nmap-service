using Vantage.Contracts.Scans;
using Vantage.Contracts.Scans.Messages;

namespace Vantage.Application.Worker.Mappers;

public static class ScanMapper
{
    public static NmapScanDto ToDto(this ScanRequestMessage scan)
        => new(scan.ScanId, scan.Target);
}