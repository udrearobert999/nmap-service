namespace Vantage.Contracts.Scans.Messages;

public record ScanRequestMessage(
    Guid ScanId, 
    string Target);