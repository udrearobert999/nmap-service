using Vantage.Contracts.ScanRiskAssessments.Options;
using Vantage.Domain.Entities;

namespace Vantage.Domain.Abstractions;

public interface IScanRiskAssessmentRepository : IRepository<ScanRiskAssessment, Guid>
{
    Task<IEnumerable<ScanRiskAssessment>> GetPagedAsync(
        GetScanRiskAssessmentsOptionsDto options,
        CancellationToken cancellationToken = default);

    Task<ScanRiskAssessment?> GetLatestByScanAsync(Guid scanId, CancellationToken cancellationToken = default);

    Task<ScanRiskAssessment?> GetWithFindingsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DateTime>> GetRequestedAtInRangeAsync(DateTime fromUtc, DateTime toUtc,
        CancellationToken cancellationToken = default);

    Task<int> CountByStatusAsync(string? status, CancellationToken cancellationToken = default);

    Task<double?> GetAverageOverallRiskScoreAsync(CancellationToken cancellationToken = default);
}
