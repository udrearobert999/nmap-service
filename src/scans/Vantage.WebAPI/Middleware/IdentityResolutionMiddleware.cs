using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vantage.Application.Services.Abstractions;
using Vantage.Application.Services.Models;
using Vantage.WebAPI.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Vantage.WebAPI.Middleware;

public class IdentityResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public IdentityResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(
        HttpContext context,
        CurrentTeamAccessor currentTeamAccessor,
        CurrentUserAccessor currentUserAccessor,
        IIdentitySyncService identitySyncService)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var userExternalId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? context.User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userExternalId))
        {
            await _next(context);
            return;
        }

        var clerkIdentity = new ClerkIdentity(
            userExternalId,
            context.User.FindFirst("email")?.Value ?? context.User.FindFirst(ClaimTypes.Email)?.Value,
            context.User.FindFirst("name")?.Value,
            context.User.FindFirst("org_id")?.Value,
            context.User.FindFirst("org_role")?.Value);

        var identity = await identitySyncService.SyncAsync(clerkIdentity, context.RequestAborted);

        currentUserAccessor.UserId = identity.UserId;
        currentUserAccessor.Role = identity.Role;
        currentTeamAccessor.TeamId = identity.TeamId;

        if (identity.TeamId is null && RequiresAuthorization(context))
        {
            await WriteNoActiveOrgAsync(context);
            return;
        }

        await _next(context);
    }

    private static bool RequiresAuthorization(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        return endpoint is not null && endpoint.Metadata.GetMetadata<IAllowAnonymous>() is null;
    }

    private static async Task WriteNoActiveOrgAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Title = "No active organization.",
            Detail = "An active organization must be selected before accessing team resources.",
            Status = StatusCodes.Status403Forbidden
        };

        var json = JsonConvert.SerializeObject(problem, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });

        await context.Response.WriteAsync(json);
    }
}
