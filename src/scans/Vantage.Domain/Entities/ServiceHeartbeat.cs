namespace Vantage.Domain.Entities;

public sealed class ServiceHeartbeat
{
    public required string ServiceName { get; init; }
    public DateTime LastSeenAt { get; set; }
}
