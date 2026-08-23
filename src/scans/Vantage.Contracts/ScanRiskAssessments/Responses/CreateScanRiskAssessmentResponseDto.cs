namespace Vantage.Contracts.ScanRiskAssessments.Responses;

public record CreateScanRiskAssessmentResponseDto(
    Guid Id,
    Guid ScanId,
    string Status,
    DateTime RequestedAt,
    DateTime? CompletedAt);
