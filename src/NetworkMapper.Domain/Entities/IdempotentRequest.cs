using NetworkMapper.Domain.Entities.Abstractions;

namespace NetworkMapper.Domain.Entities;

public sealed class IdempotentRequest : IEntity<Guid>
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required DateTime CreatedAt { get; init; }
}