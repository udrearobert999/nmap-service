namespace NetworkMapper.Application.Dtos.Scans.Responses;

public record PaginatedListResponseDto<T>(
    IEnumerable<T> Items, 
    int Total);