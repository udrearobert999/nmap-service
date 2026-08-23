using Vantage.Application.Validation.Abstractions;
using Vantage.Contracts.ScanRiskAssessments.Options;
using Vantage.Domain.Entities;

namespace Vantage.Application.Validation.ScanRiskAssessments.Options;

public class GetScanRiskAssessmentsOptionsDtoValidator :
    BaseGetCollectionOptionsDtoValidator<GetScanRiskAssessmentsOptionsDto, ScanRiskAssessment>
{
    public GetScanRiskAssessmentsOptionsDtoValidator()
    {
    }
}
