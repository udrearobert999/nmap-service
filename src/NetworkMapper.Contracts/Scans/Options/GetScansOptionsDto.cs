using NetworkMapper.Contracts.Abstractions;

namespace NetworkMapper.Contracts.Scans.Options;

public record GetScansOptionsDto(
    string? Target,
    int? Page,
    int? PageSize,
    string? OrderBy,
    string? OrderDirection) 
    : BaseGetCollectionOptionsDto(Page, PageSize, OrderBy, OrderDirection);