namespace NetworkMapper.Contracts.ScanRiskAssessments;

public record ScanRiskAssessmentDto(
    Guid Id,
    Guid ScanId,
    string Status,
    DateTime RequestedAt,
    DateTime? CompletedAt,
    string? ErrorMessage,
    double? OverallRiskScore,
    string? CreatedByEmail);
