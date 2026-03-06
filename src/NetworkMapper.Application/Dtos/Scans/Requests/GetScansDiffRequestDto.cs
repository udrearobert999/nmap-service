namespace NetworkMapper.Application.Dtos.Scans.Requests;

public record GetScansDiffRequestDto(
    string Target, 
    string From, 
    string To);