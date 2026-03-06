namespace NetworkMapper.Contracts.Scans.Responses;

public record PaginatedListResponseDto<T>(
    IEnumerable<T> Items, 
    int Total);