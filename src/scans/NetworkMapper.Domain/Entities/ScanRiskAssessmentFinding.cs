using NetworkMapper.Domain.Entities.Abstractions;

namespace NetworkMapper.Domain.Entities;

public sealed class ScanRiskAssessmentFinding : IEntity<Guid>
{
    public Guid Id { get; init; }

    public Guid ScanRiskAssessmentId { get; init; }
    public ScanRiskAssessment ScanRiskAssessment { get; init; } = null!;

    public int Port { get; init; }
    public required string Service { get; init; }
    public string? Product { get; init; }
    public string? Version { get; init; }
    public string? Cpe { get; init; }
    public required string MatchedCves { get; init; }
    public double CvssScore { get; init; }
    public bool KevFlag { get; init; }
    public double MatchConfidence { get; init; }
}
