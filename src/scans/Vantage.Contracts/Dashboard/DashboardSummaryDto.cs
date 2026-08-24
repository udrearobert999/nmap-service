namespace Vantage.Contracts.Dashboard;

public sealed record DashboardSummaryDto(
    int TotalScans,
    int TotalAssessments,
    int CompletedAssessments,
    double? AvgRiskScore);
