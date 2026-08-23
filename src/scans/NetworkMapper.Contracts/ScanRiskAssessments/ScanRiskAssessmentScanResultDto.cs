namespace NetworkMapper.Contracts.ScanRiskAssessments;

public record ScanRiskAssessmentScanResultDto(
    int Port,
    string Service,
    string? Product,
    string? Version);
