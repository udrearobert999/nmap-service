namespace NetworkMapper.Contracts.Scans.Responses;

public record GetScansResponseDto(
    IEnumerable<ScanDto> Items, 
    int Total) : PaginatedListResponseDto<ScanDto>(Items, Total);