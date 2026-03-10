using NetworkMapper.Domain.Entities.Abstractions;

namespace NetworkMapper.Domain.Entities;

public sealed class OutboxMessage : IEntity<Guid>
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public required string Status { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
}