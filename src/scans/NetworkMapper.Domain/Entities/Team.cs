using NetworkMapper.Domain.Entities.Abstractions;

namespace NetworkMapper.Domain.Entities;

public sealed class Team : IEntity<Guid>
{
    public Guid Id { get; init; }
    public required string ClerkOrgId { get; init; }
    public string? Name { get; set; }
    public required DateTime CreatedAt { get; init; }
}
