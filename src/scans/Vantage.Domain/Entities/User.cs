using Vantage.Domain.Entities.Abstractions;

namespace Vantage.Domain.Entities;

public sealed class User : IEntity<Guid>
{
    public Guid Id { get; init; }
    public required string ClerkUserId { get; init; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public required DateTime CreatedAt { get; init; }
}
