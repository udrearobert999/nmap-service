namespace Vantage.Domain.Entities;

public sealed class CveCacheEntry
{
    public required string CpeUri { get; init; }
    public required string CveId { get; init; }
    public double CvssScore { get; init; }
    public DateTime CachedAt { get; init; }
}
