using NetworkMapper.Contracts.Ports;
using NetworkMapper.Contracts.Scans;

namespace NetworkMapper.Application.Services.Models;

internal sealed record PortAnalysisResult(
    List<ScanResultDto> Added,
    List<PortStateChangeDto> Changed,
    List<ScanResultDto> Unchanged);