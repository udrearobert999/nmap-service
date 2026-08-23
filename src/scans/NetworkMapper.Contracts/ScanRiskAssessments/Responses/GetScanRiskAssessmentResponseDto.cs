namespace NetworkMapper.Contracts.ScanRiskAssessments.Responses;

public record GetScanRiskAssessmentResponseDto(
    Guid Id,
    Guid ScanId,
    string Status,
    DateTime RequestedAt,
    DateTime? CompletedAt,
    string? ErrorMessage,
    double? OverallRiskScore,
    string? CreatedByEmail,
    IEnumerable<ScanRiskAssessmentFindingDto> Findings);
