namespace NetworkMapper.Application.Dtos.Abstractions;

public abstract record BaseGetCollectionOptionsDto(
    int? Page = 1, 
    int? PageSize = 10, 
    string? OrderBy = null, 
    string? OrderDirection = "ASC");