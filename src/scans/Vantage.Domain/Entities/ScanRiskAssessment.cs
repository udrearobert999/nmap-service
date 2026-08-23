using Vantage.Domain.Entities.Abstractions;

namespace Vantage.Domain.Entities;

public sealed class ScanRiskAssessment : IEntity<Guid>, IIdempotentEntity
{
    public Guid Id { get; init; }
    public Guid RequestId { get; init; }
    public Guid TeamId { get; init; }
    public Guid CreatedByUserId { get; init; }
    public User? CreatedByUser { get; init; }

    public Guid ScanId { get; init; }
    public Scan Scan { get; init; } = null!;

    public required string Status { get; set; }
    public string? ErrorMessage { get; set; }
    public required DateTime RequestedAt { get; init; }
    public DateTime? CompletedAt { get; set; }
    public double? OverallRiskScore { get; set; }

    public ICollection<ScanRiskAssessmentFinding> Findings { get; init; } = new List<ScanRiskAssessmentFinding>();
}
