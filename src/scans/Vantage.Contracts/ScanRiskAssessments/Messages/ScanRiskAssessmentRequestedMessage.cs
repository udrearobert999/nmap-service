namespace Vantage.Contracts.ScanRiskAssessments.Messages;

public record ScanRiskAssessmentRequestedMessage(
    Guid ScanRiskAssessmentId,
    Guid ScanId,
    Guid TeamId,
    IReadOnlyCollection<ScanRiskAssessmentScanResultDto> Results);
