namespace Vantage.Application.Services.Models;

public sealed record ClerkIdentity(
    string UserExternalId,
    string? Email,
    string? Name,
    string? OrgExternalId,
    string? OrgRole);
