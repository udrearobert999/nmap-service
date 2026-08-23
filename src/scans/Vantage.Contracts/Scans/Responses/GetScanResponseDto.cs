namespace Vantage.Contracts.Scans.Responses;

public record GetScanResponseDto(
    Guid Id,
    string Target,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? ErrorMessage,
    string? CreatedByEmail,
    IEnumerable<ScanResultDto> Results);