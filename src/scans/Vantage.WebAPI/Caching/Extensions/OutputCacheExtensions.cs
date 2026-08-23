using Microsoft.AspNetCore.OutputCaching;
using Vantage.Application.Shared.Helpers;
using Vantage.Contracts.ScanRiskAssessments.Options;
using Vantage.Contracts.Scans.Options;
using Vantage.WebAPI.Caching.Constants;

namespace Vantage.WebAPI.Caching.Extensions;

internal static class OutputCacheExtensions
{
    public static OutputCacheOptions ConfigureCustomPolicies(this OutputCacheOptions options)
    {
        options.AddPolicy(CacheConstants.Policies.Scans, policy =>
            policy.Cache()
                .Expire(TimeSpan.FromMinutes(1))
                .SetVaryByQueryByTypeProperties<GetScansOptionsDto>()
                .Tag(CacheConstants.Keys.Scans));

        options.AddPolicy(CacheConstants.Policies.ScanRiskAssessments, policy =>
            policy.Cache()
                .Expire(TimeSpan.FromMinutes(1))
                .SetVaryByQueryByTypeProperties<GetScanRiskAssessmentsOptionsDto>()
                .Tag(CacheConstants.Keys.ScanRiskAssessments));

        return options;
    }

    public static OutputCachePolicyBuilder SetVaryByQueryByTypeProperties<T>(this OutputCachePolicyBuilder builder)
    {
        var jsonPropertiesOfType = ReflectionHelper.GetProperties<T>();

        builder.SetVaryByQuery(jsonPropertiesOfType);

        return builder;
    }
}