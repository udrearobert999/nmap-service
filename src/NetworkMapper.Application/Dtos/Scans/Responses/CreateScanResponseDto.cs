namespace NetworkMapper.Application.Dtos.Scans.Responses;

public record CreateScanResponseDto(
    Guid Id, 
    string Target, 
    string Status, 
    DateTime CreatedAt);