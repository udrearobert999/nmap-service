namespace Vantage.Contracts.Scans;

public record ScanDto(
    Guid Id,
    string Target,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? ErrorMessage,
    string? CreatedByEmail,
    IEnumerable<ScanResultDto> Results);