namespace NetworkMapper.Contracts.Scans.Requests;

public record GetScansDiffRequestDto(
    string Target,
    Guid? From,
    Guid? To);