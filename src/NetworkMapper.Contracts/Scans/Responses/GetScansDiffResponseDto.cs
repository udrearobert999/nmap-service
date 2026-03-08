using NetworkMapper.Contracts.Ports;

namespace NetworkMapper.Contracts.Scans.Responses;

public record GetScansDiffResponseDto(
    IEnumerable<ScanResultDto> AddedPorts,
    IEnumerable<ScanResultDto> RemovedPorts,
    IEnumerable<PortStateChangeDto> ChangedPorts,
    IEnumerable<ScanResultDto> UnchangedPorts);