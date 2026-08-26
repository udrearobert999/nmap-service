using System.Text.Json;
using Vantage.Application.Services.Abstractions;
using Vantage.Domain.Abstractions;
using Vantage.Domain.Entities;

namespace Vantage.Application.Services;

internal sealed class ClerkWebhookService : IClerkWebhookService
{
    private const string RolePrefix = "org:";
    private const string DefaultRole = "member";

    private readonly IUnitOfWork _unitOfWork;

    public ClerkWebhookService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(
        string eventType,
        JsonElement data,
        CancellationToken cancellationToken = default)
    {
        switch (eventType)
        {
            case "user.created":
            case "user.updated":
                await UpsertUserAsync(data, cancellationToken);
                return true;

            case "user.deleted":
                await RevokeUserAccessAsync(data, cancellationToken);
                return true;

            case "organization.created":
            case "organization.updated":
                await UpsertTeamAsync(data, cancellationToken);
                return true;

            case "organization.deleted":
                await DeleteTeamAsync(data, cancellationToken);
                return true;

            case "organizationMembership.created":
            case "organizationMembership.updated":
                await UpsertMembershipAsync(data, cancellationToken);
                return true;

            case "organizationMembership.deleted":
                await DeleteMembershipAsync(data, cancellationToken);
                return true;

            default:
                return false;
        }
    }

    private async Task<Guid?> UpsertUserAsync(JsonElement data, CancellationToken cancellationToken)
    {
        var clerkUserId = ReadString(data, "id");
        if (string.IsNullOrWhiteSpace(clerkUserId))
        {
            return null;
        }

        return await UpsertUserAsync(clerkUserId, ReadPrimaryEmail(data), ReadFullName(data), cancellationToken);
    }

    private async Task<Guid> UpsertUserAsync(
        string clerkUserId,
        string? email,
        string? name,
        CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Users
            .FirstOrDefaultAsync(u => u.ClerkUserId == clerkUserId, cancellationToken, track: true);

        if (existing is not null)
        {
            var changed = false;

            if (!string.IsNullOrWhiteSpace(email) && existing.Email != email)
            {
                existing.Email = email;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(name) && existing.Name != name)
            {
                existing.Name = name;
                changed = true;
            }

            if (changed)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return existing.Id;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            ClerkUserId = clerkUserId,
            Email = email,
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.CreateAsync(user, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return user.Id;
        }
        catch (Exception ex) when (_unitOfWork.IsUniqueConstraintViolation(ex))
        {
            _unitOfWork.Detach(user);

            var raced = await _unitOfWork.Users
                .FirstOrDefaultAsync(u => u.ClerkUserId == clerkUserId, cancellationToken);

            return raced?.Id ?? throw new InvalidOperationException(
                "User insert lost a race but the winning row could not be read back.");
        }
    }

    private async Task RevokeUserAccessAsync(JsonElement data, CancellationToken cancellationToken)
    {
        var clerkUserId = ReadString(data, "id");
        if (string.IsNullOrWhiteSpace(clerkUserId))
        {
            return;
        }

        var user = await _unitOfWork.Users
            .FirstOrDefaultAsync(u => u.ClerkUserId == clerkUserId, cancellationToken);

        if (user is null)
        {
            return;
        }

        var memberships = await _unitOfWork.TeamMemberships
            .GetByExpressionAsync(m => m.UserId == user.Id, cancellationToken, track: true);

        var removed = false;
        foreach (var membership in memberships)
        {
            await _unitOfWork.TeamMemberships.DeleteAsync(membership, cancellationToken);
            removed = true;
        }

        if (removed)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<Guid?> UpsertTeamAsync(JsonElement data, CancellationToken cancellationToken)
    {
        var clerkOrgId = ReadString(data, "id");
        if (string.IsNullOrWhiteSpace(clerkOrgId))
        {
            return null;
        }

        return await UpsertTeamAsync(clerkOrgId, ReadString(data, "name"), cancellationToken);
    }

    private async Task<Guid> UpsertTeamAsync(
        string clerkOrgId,
        string? name,
        CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Teams
            .FirstOrDefaultAsync(t => t.ClerkOrgId == clerkOrgId, cancellationToken, track: true);

        if (existing is not null)
        {
            if (!string.IsNullOrWhiteSpace(name) && existing.Name != name)
            {
                existing.Name = name;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return existing.Id;
        }

        var team = new Team
        {
            Id = Guid.NewGuid(),
            ClerkOrgId = clerkOrgId,
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Teams.CreateAsync(team, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return team.Id;
        }
        catch (Exception ex) when (_unitOfWork.IsUniqueConstraintViolation(ex))
        {
            _unitOfWork.Detach(team);

            var raced = await _unitOfWork.Teams
                .FirstOrDefaultAsync(t => t.ClerkOrgId == clerkOrgId, cancellationToken);

            return raced?.Id ?? throw new InvalidOperationException(
                "Team insert lost a race but the winning row could not be read back.");
        }
    }

    private async Task DeleteTeamAsync(JsonElement data, CancellationToken cancellationToken)
    {
        var clerkOrgId = ReadString(data, "id");
        if (string.IsNullOrWhiteSpace(clerkOrgId))
        {
            return;
        }

        var team = await _unitOfWork.Teams
            .FirstOrDefaultAsync(t => t.ClerkOrgId == clerkOrgId, cancellationToken, track: true);

        if (team is null)
        {
            return;
        }

        await _unitOfWork.Teams.DeleteAsync(team, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertMembershipAsync(JsonElement data, CancellationToken cancellationToken)
    {
        var clerkOrgId = ReadNestedString(data, "organization", "id");
        var clerkUserId = ReadNestedString(data, "public_user_data", "user_id");

        if (string.IsNullOrWhiteSpace(clerkOrgId) || string.IsNullOrWhiteSpace(clerkUserId))
        {
            return;
        }

        var teamId = await UpsertTeamAsync(
            clerkOrgId,
            ReadNestedString(data, "organization", "name"),
            cancellationToken);

        var userId = await UpsertUserAsync(
            clerkUserId,
            ReadNestedString(data, "public_user_data", "identifier"),
            ReadNestedFullName(data, "public_user_data"),
            cancellationToken);

        var role = NormalizeRole(ReadString(data, "role"));

        var existing = await _unitOfWork.TeamMemberships
            .FirstOrDefaultAsync(m => m.UserId == userId && m.TeamId == teamId, cancellationToken, track: true);

        if (existing is not null)
        {
            if (existing.Role != role)
            {
                existing.Role = role;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        var membership = new TeamMembership
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TeamId = teamId,
            Role = role
        };

        await _unitOfWork.TeamMemberships.CreateAsync(membership, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (_unitOfWork.IsUniqueConstraintViolation(ex))
        {
            _unitOfWork.Detach(membership);
        }
    }

    private async Task DeleteMembershipAsync(JsonElement data, CancellationToken cancellationToken)
    {
        var clerkOrgId = ReadNestedString(data, "organization", "id");
        var clerkUserId = ReadNestedString(data, "public_user_data", "user_id");

        if (string.IsNullOrWhiteSpace(clerkOrgId) || string.IsNullOrWhiteSpace(clerkUserId))
        {
            return;
        }

        var team = await _unitOfWork.Teams
            .FirstOrDefaultAsync(t => t.ClerkOrgId == clerkOrgId, cancellationToken);
        var user = await _unitOfWork.Users
            .FirstOrDefaultAsync(u => u.ClerkUserId == clerkUserId, cancellationToken);

        if (team is null || user is null)
        {
            return;
        }

        var membership = await _unitOfWork.TeamMemberships
            .FirstOrDefaultAsync(m => m.UserId == user.Id && m.TeamId == team.Id, cancellationToken, track: true);

        if (membership is null)
        {
            return;
        }

        await _unitOfWork.TeamMemberships.DeleteAsync(membership, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return DefaultRole;
        }

        return role.StartsWith(RolePrefix, StringComparison.OrdinalIgnoreCase)
            ? role[RolePrefix.Length..]
            : role;
    }

    private static string? ReadString(JsonElement element, string property)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static string? ReadNestedString(JsonElement element, string parent, string property)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return element.TryGetProperty(parent, out var nested)
            ? ReadString(nested, property)
            : null;
    }

    private static string? ReadPrimaryEmail(JsonElement data)
    {
        if (data.ValueKind != JsonValueKind.Object ||
            !data.TryGetProperty("email_addresses", out var addresses) ||
            addresses.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var primaryId = ReadString(data, "primary_email_address_id");

        foreach (var address in addresses.EnumerateArray())
        {
            if (primaryId is not null && ReadString(address, "id") == primaryId)
            {
                return ReadString(address, "email_address");
            }
        }

        foreach (var address in addresses.EnumerateArray())
        {
            var email = ReadString(address, "email_address");
            if (!string.IsNullOrWhiteSpace(email))
            {
                return email;
            }
        }

        return null;
    }

    private static string? ReadFullName(JsonElement data) =>
        JoinName(ReadString(data, "first_name"), ReadString(data, "last_name"));

    private static string? ReadNestedFullName(JsonElement data, string parent)
    {
        if (data.ValueKind != JsonValueKind.Object || !data.TryGetProperty(parent, out var nested))
        {
            return null;
        }

        return JoinName(ReadString(nested, "first_name"), ReadString(nested, "last_name"));
    }

    private static string? JoinName(string? first, string? last)
    {
        var full = $"{first} {last}".Trim();
        return string.IsNullOrWhiteSpace(full) ? null : full;
    }
}
