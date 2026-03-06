namespace NetworkMapper.Application.Dtos.Scans;

public record ScanDto(
    Guid Id, 
    string Target, 
    string Status, 
    DateTime CreatedAt);