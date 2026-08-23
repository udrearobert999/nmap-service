namespace Vantage.Contracts.Scans.Requests;

public record GetScansDiffRequestDto(
    string Target,
    Guid? From,
    Guid? To);