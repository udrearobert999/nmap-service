using Vantage.Application.Services.Abstractions;
using Vantage.Contracts.Dashboard;
using Vantage.Domain.Abstractions;

namespace Vantage.Application.Services;

internal sealed class DashboardService : IDashboardService
{
    private const string HourBucket = "hour";

    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ActivityPointDto>> GetActivityAsync(
        DateTime fromUtc,
        DateTime toUtc,
        string bucket,
        CancellationToken cancellationToken = default)
    {
        var isHour = string.Equals(bucket, HourBucket, StringComparison.OrdinalIgnoreCase);

        var scanTimes = await _unitOfWork.Scans.GetCreatedAtInRangeAsync(fromUtc, toUtc, cancellationToken);
        var assessmentTimes =
            await _unitOfWork.ScanRiskAssessments.GetRequestedAtInRangeAsync(fromUtc, toUtc, cancellationToken);

        var scanCounts = BucketCounts(scanTimes, isHour);
        var assessmentCounts = BucketCounts(assessmentTimes, isHour);

        var points = new List<ActivityPointDto>();
        for (var cursor = Truncate(fromUtc, isHour); cursor < toUtc; cursor = Step(cursor, isHour))
        {
            scanCounts.TryGetValue(cursor, out var scans);
            assessmentCounts.TryGetValue(cursor, out var assessments);
            points.Add(new ActivityPointDto(cursor, scans, assessments));
        }

        return points;
    }

    private static Dictionary<DateTime, int> BucketCounts(IReadOnlyList<DateTime> times, bool isHour)
    {
        var counts = new Dictionary<DateTime, int>();
        foreach (var time in times)
        {
            var key = Truncate(time, isHour);
            counts[key] = counts.TryGetValue(key, out var current) ? current + 1 : 1;
        }

        return counts;
    }

    private static DateTime Truncate(DateTime value, bool isHour)
    {
        var utc = DateTime.SpecifyKind(value, DateTimeKind.Utc);

        return isHour
            ? new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, DateTimeKind.Utc)
            : new DateTime(utc.Year, utc.Month, utc.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    private static DateTime Step(DateTime value, bool isHour) =>
        isHour ? value.AddHours(1) : value.AddDays(1);
}
