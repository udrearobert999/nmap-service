using NetworkMapper.Domain.Entities.Abstractions;

namespace NetworkMapper.Domain.Entities;

public sealed class TeamMembership : IEntity<Guid>
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }
    public User User { get; init; } = null!;

    public Guid TeamId { get; init; }
    public Team Team { get; init; } = null!;

    public required string Role { get; set; }
}
