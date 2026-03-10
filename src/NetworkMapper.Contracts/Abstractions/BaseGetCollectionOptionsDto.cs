namespace NetworkMapper.Contracts.Abstractions;

public abstract record BaseGetCollectionOptionsDto(
    int? PageNumber = 1, 
    int? PageSize = 10, 
    string? OrderBy = null, 
    string? OrderDirection = "ASC");