using Vantage.Contracts.Scans.Responses;

namespace Vantage.Contracts.ScanRiskAssessments.Responses;

public record GetScanRiskAssessmentsResponseDto(
    IEnumerable<ScanRiskAssessmentDto> Items,
    int Total) : PaginatedListResponseDto<ScanRiskAssessmentDto>(Items, Total);
