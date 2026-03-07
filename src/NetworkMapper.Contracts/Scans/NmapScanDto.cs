namespace NetworkMapper.Contracts.Scans;

public record NmapScanDto(
    Guid Id,
    string Target);