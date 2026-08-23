namespace NetworkMapper.Contracts.Scans.Options;

public record GetScansDiffOptions(
    Guid? From,
    Guid? To
);