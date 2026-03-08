namespace NetworkMapper.Contracts.Scans;

public record ScanResultDto(
    int Port,
    string Protocol,
    string Service,
    string State);