namespace NetworkMapper.WebAPI.Caching.Constants;

internal static class CacheConstants
{
    internal static class Policies
    {
        internal const string Scans = "ScansCachePolicy";
        internal const string ScanRiskAssessments = "ScanRiskAssessmentsCachePolicy";
    }

    internal static class Keys
    {
        internal const string Scans = "scans";
        internal const string ScanRiskAssessments = "scan-risk-assessments";
    }
}