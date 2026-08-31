namespace Vantage.Contracts.ScanRiskAssessments.Responses;

public record RiskTrendPointDto(
    Guid ScanId,
    Guid ScanRiskAssessmentId,
    DateTime CompletedAt,
    double? OverallRiskScore);
