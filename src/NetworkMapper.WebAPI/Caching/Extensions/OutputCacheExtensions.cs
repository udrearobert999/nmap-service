using Microsoft.AspNetCore.OutputCaching;
using NetworkMapper.Application.Dtos.Scans.Options;
using NetworkMapper.Application.Shared.Helpers;
using NetworkMapper.WebAPI.Caching.Constants;

namespace NetworkMapper.WebAPI.Caching.Extensions;

internal static class OutputCacheExtensions
{
    public static OutputCacheOptions ConfigureCustomPolicies(this OutputCacheOptions options)
    {
        options.AddBasePolicy(policy => policy.Cache());
        options.AddPolicy(CacheConstants.Policies.Scans, policy =>
            policy.Cache()
                .Expire(TimeSpan.FromMinutes(1))
                .SetVaryByQueryByTypeProperties<GetAllScansOptionsDto>()
                .Tag(CacheConstants.Keys.Scans));

        return options;
    }

    public static OutputCachePolicyBuilder SetVaryByQueryByTypeProperties<T>(this OutputCachePolicyBuilder builder)
    {
        var jsonPropertiesOfType = ReflectionHelper.GetProperties<T>();

        builder.SetVaryByQuery(jsonPropertiesOfType);

        return builder;
    }
}