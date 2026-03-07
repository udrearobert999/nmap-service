namespace NetworkMapper.Infrastructure.Persistence.Constants;

internal static class TableNamesConstants
{
    internal const string Scans = nameof(Scans);
    internal const string IdempotentRequests = nameof(IdempotentRequests);
    internal const string ScansResults = nameof(ScansResults);
    internal const string OutboxMessages = nameof(OutboxMessages);
}