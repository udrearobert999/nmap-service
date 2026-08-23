using NetworkMapper.Contracts.ScanRiskAssessments.Options;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Domain.Abstractions;

public interface IScanRiskAssessmentRepository : IRepository<ScanRiskAssessment, Guid>
{
    Task<IEnumerable<ScanRiskAssessment>> GetPagedAsync(
        GetScanRiskAssessmentsOptionsDto options,
        CancellationToken cancellationToken = default);

    Task<ScanRiskAssessment?> GetLatestByScanAsync(Guid scanId, CancellationToken cancellationToken = default);

    Task<ScanRiskAssessment?> GetWithFindingsByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
