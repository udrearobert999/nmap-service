using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vantage.Application.Services.Abstractions;
using Vantage.Contracts.Dashboard;

namespace Vantage.WebAPI.Controllers;

[Authorize]
public class DashboardController : ApiControllerBase
{
    private static readonly TimeSpan MaxDaySpan = TimeSpan.FromDays(90);
    private static readonly TimeSpan MaxHourSpan = TimeSpan.FromDays(3);

    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("activity")]
    [ProducesResponseType(typeof(IReadOnlyList<ActivityPointDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetActivity(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] string? bucket,
        CancellationToken cancellationToken)
    {
        var toUtc = (to ?? DateTimeOffset.UtcNow).UtcDateTime;
        var fromUtc = (from ?? DateTimeOffset.UtcNow.AddDays(-14)).UtcDateTime;

        var normalizedBucket =
            string.Equals(bucket, "hour", StringComparison.OrdinalIgnoreCase) ? "hour" : "day";

        if (fromUtc >= toUtc)
            fromUtc = toUtc.AddDays(-14);

        var maxSpan = normalizedBucket == "hour" ? MaxHourSpan : MaxDaySpan;
        if (toUtc - fromUtc > maxSpan)
            fromUtc = toUtc - maxSpan;

        var points = await _dashboardService.GetActivityAsync(fromUtc, toUtc, normalizedBucket, cancellationToken);

        return Ok(points);
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _dashboardService.GetSummaryAsync(cancellationToken);

        return Ok(summary);
    }
}
