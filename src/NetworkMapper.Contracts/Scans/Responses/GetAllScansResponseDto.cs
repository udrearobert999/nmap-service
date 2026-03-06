namespace NetworkMapper.Contracts.Scans.Responses;

public record GetAllScansResponseDto(
    IEnumerable<ScanDto> Items, 
    int Total) : PaginatedListResponseDto<ScanDto>(Items, Total);