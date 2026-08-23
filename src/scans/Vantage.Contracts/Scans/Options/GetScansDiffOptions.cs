namespace Vantage.Contracts.Scans.Options;

public record GetScansDiffOptions(
    Guid? From,
    Guid? To
);