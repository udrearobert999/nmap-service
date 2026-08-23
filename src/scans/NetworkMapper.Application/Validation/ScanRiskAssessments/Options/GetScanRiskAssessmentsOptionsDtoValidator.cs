using NetworkMapper.Application.Validation.Abstractions;
using NetworkMapper.Contracts.ScanRiskAssessments.Options;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Application.Validation.ScanRiskAssessments.Options;

public class GetScanRiskAssessmentsOptionsDtoValidator :
    BaseGetCollectionOptionsDtoValidator<GetScanRiskAssessmentsOptionsDto, ScanRiskAssessment>
{
    public GetScanRiskAssessmentsOptionsDtoValidator()
    {
    }
}
