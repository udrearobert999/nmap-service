using Vantage.WebAPI.Middleware;

namespace Vantage.WebAPI.Extensions;

public static class IdentityResolutionMiddlewareExtension
{
    public static IApplicationBuilder UseIdentityResolution(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<IdentityResolutionMiddleware>();

        return builder;
    }
}
