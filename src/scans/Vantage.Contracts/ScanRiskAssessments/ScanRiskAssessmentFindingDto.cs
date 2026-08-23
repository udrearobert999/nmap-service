namespace Vantage.Contracts.ScanRiskAssessments;

public record ScanRiskAssessmentFindingDto(
    int Port,
    string Service,
    string? Product,
    string? Version,
    string? Cpe,
    IEnumerable<string> MatchedCves,
    double CvssScore,
    bool KevFlag,
    double MatchConfidence);
