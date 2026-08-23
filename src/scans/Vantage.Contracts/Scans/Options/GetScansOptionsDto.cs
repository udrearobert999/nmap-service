using Vantage.Contracts.Abstractions;

namespace Vantage.Contracts.Scans.Options;

public record GetScansOptionsDto(
    string? Target,
    int? PageNumber,
    int? PageSize,
    string? OrderBy,
    string? OrderDirection) 
    : BaseGetCollectionOptionsDto(PageNumber, PageSize, OrderBy, OrderDirection);