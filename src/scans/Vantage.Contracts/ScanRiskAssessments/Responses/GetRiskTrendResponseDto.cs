namespace Vantage.Contracts.ScanRiskAssessments.Responses;

public record GetRiskTrendResponseDto(string Target, IEnumerable<RiskTrendPointDto> Points);
