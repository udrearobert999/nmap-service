using NetworkMapper.Domain.Entities.Abstractions;

namespace NetworkMapper.Domain.Entities;

public sealed class Scan : IEntity<Guid>, IIdempotentEntity
{
    public Guid Id { get; init; }
    public Guid RequestId { get; init; }
    public required string Target { get; init; }
    public required string Status { get; init; }
    public required DateTime CreatedAt { get; init; }
}