namespace NetworkMapper.Contracts.Scans.Requests;

public record GetScansDiffRequestDto(
    string Target, 
    string From, 
    string To);