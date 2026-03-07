using NetworkMapper.Domain.Entities.Abstractions;

namespace NetworkMapper.Domain.Entities;

public sealed class Scan : IEntity<Guid>, IIdempotentEntity
{
    public Guid Id { get; init; }
    public Guid RequestId { get; init; }
    public required string Target { get; init; }
    public required string Status { get; set; }
    public string? ErrorMessage { get; set; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; set; }
    public ICollection<ScanResult> Results { get; init; } = new List<ScanResult>();
}