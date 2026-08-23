namespace Vantage.Contracts.Scans.Responses;

public record CreateScanResponseDto(
    Guid Id, 
    string Target, 
    string Status, 
    DateTime CreatedAt,
    DateTime? CompletedAt);