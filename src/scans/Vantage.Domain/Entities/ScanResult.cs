using Vantage.Domain.Entities.Abstractions;

namespace Vantage.Domain.Entities;

public sealed class ScanResult : IEntity<Guid>
{
    public Guid Id { get; init; }

    public Guid ScanId { get; init; }
    public Scan Scan { get; init; } = null!;

    public int Port { get; init; }
    public required string Protocol { get; init; }
    public required string Service { get; init; }
    public required string State { get; init; }
    public string? Product { get; init; }
    public string? Version { get; init; }
}