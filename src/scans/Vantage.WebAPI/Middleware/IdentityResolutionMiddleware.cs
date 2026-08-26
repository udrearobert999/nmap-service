using System.Security.Claims;
using System.Text.Json;
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

        var (orgId, orgRole) = ResolveOrganization(context.User);

        var clerkIdentity = new ClerkIdentity(
            userExternalId,
            context.User.FindFirst("email")?.Value
            ?? context.User.FindFirst("email_address")?.Value
            ?? context.User.FindFirst(ClaimTypes.Email)?.Value,
            context.User.FindFirst("name")?.Value
            ?? context.User.FindFirst("full_name")?.Value
            ?? context.User.FindFirst(ClaimTypes.Name)?.Value,
            orgId,
            orgRole);

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

    private static (string? OrgId, string? OrgRole) ResolveOrganization(ClaimsPrincipal user)
    {
        var flatOrgId = user.FindFirst("org_id")?.Value;
        if (!string.IsNullOrWhiteSpace(flatOrgId))
        {
            return (flatOrgId, NormalizeRole(user.FindFirst("org_role")?.Value));
        }

        var organizationClaim = user.FindFirst("o")?.Value;
        if (string.IsNullOrWhiteSpace(organizationClaim))
        {
            return (null, null);
        }

        try
        {
            using var document = JsonDocument.Parse(organizationClaim);
            var root = document.RootElement;

            var id = root.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
            var role = root.TryGetProperty("rol", out var roleElement) ? roleElement.GetString() : null;

            return (id, NormalizeRole(role));
        }
        catch (System.Text.Json.JsonException)
        {
            return (null, null);
        }
    }

    private static string? NormalizeRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return null;
        }

        return role.StartsWith("org:", StringComparison.OrdinalIgnoreCase)
            ? role["org:".Length..]
            : role;
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
