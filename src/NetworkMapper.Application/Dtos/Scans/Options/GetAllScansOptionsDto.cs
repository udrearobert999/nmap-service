using NetworkMapper.Application.Dtos.Abstractions;

namespace NetworkMapper.Application.Dtos.Scans.Options;

public record GetAllScansOptionsDto(
    string? Target,
    int? Page,
    int? PageSize,
    string? OrderBy,
    string? OrderDirection) 
    : BaseGetCollectionOptionsDto(Page, PageSize, OrderBy, OrderDirection);