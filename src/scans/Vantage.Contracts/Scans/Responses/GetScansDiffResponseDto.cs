using Vantage.Contracts.Ports;

namespace Vantage.Contracts.Scans.Responses;

public record GetScansDiffResponseDto(
    IEnumerable<ScanResultDto> AddedPorts,
    IEnumerable<ScanResultDto> RemovedPorts,
    IEnumerable<PortStateChangeDto> ChangedPorts,
    IEnumerable<ScanResultDto> UnchangedPorts);