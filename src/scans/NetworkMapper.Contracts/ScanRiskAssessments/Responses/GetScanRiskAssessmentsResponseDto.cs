using NetworkMapper.Contracts.Scans.Responses;

namespace NetworkMapper.Contracts.ScanRiskAssessments.Responses;

public record GetScanRiskAssessmentsResponseDto(
    IEnumerable<ScanRiskAssessmentDto> Items,
    int Total) : PaginatedListResponseDto<ScanRiskAssessmentDto>(Items, Total);
