namespace NetworkMapper.Contracts.Scans.Messages;

public record ScanRequestMessage(
    Guid ScanId, 
    string Target);