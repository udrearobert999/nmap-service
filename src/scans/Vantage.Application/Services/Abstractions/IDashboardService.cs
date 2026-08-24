using Vantage.Contracts.Dashboard;

namespace Vantage.Application.Services.Abstractions;

public interface IDashboardService
{
    Task<IReadOnlyList<ActivityPointDto>> GetActivityAsync(
        DateTime fromUtc,
        DateTime toUtc,
        string bucket,
        CancellationToken cancellationToken = default);

    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
