using Vantage.Contracts.Abstractions;

namespace Vantage.Contracts.ScanRiskAssessments.Options;

public record GetScanRiskAssessmentsOptionsDto(
    string? Status,
    Guid? ScanId,
    int? PageNumber,
    int? PageSize,
    string? OrderBy,
    string? OrderDirection)
    : BaseGetCollectionOptionsDto(PageNumber, PageSize, OrderBy, OrderDirection);
