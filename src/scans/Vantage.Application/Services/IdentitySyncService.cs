using Vantage.Application.Services.Abstractions;
using Vantage.Application.Services.Models;
using Vantage.Domain.Abstractions;
using Vantage.Domain.Entities;

namespace Vantage.Application.Services;

internal sealed class IdentitySyncService : IIdentitySyncService
{
    private readonly IUnitOfWork _unitOfWork;

    public IdentitySyncService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CurrentIdentity> SyncAsync(ClerkIdentity identity, CancellationToken cancellationToken = default)
    {
        var userId = await SyncUserAsync(identity, cancellationToken);

        if (string.IsNullOrWhiteSpace(identity.OrgExternalId))
        {
            return new CurrentIdentity(userId, TeamId: null, Role: null);
        }

        var teamId = await SyncTeamAsync(identity.OrgExternalId, identity.Name, cancellationToken);
        var role = string.IsNullOrWhiteSpace(identity.OrgRole) ? "member" : identity.OrgRole;
        await SyncMembershipAsync(userId, teamId, role, cancellationToken);

        return new CurrentIdentity(userId, teamId, role);
    }

    private async Task<Guid> SyncUserAsync(ClerkIdentity identity, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Users
            .FirstOrDefaultAsync(u => u.ClerkUserId == identity.UserExternalId, cancellationToken, track: true);

        if (existing is not null)
        {
            var changed = false;
            if (!string.IsNullOrWhiteSpace(identity.Email) && existing.Email != identity.Email)
            {
                existing.Email = identity.Email;
                changed = true;
            }
            if (!string.IsNullOrWhiteSpace(identity.Name) && existing.Name != identity.Name)
            {
                existing.Name = identity.Name;
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
            ClerkUserId = identity.UserExternalId,
            Email = identity.Email,
            Name = identity.Name,
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
            var raced = await _unitOfWork.Users
                .FirstOrDefaultAsync(u => u.ClerkUserId == identity.UserExternalId, cancellationToken);
            return raced!.Id;
        }
    }

    private async Task<Guid> SyncTeamAsync(string orgExternalId, string? name, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Teams
            .FirstOrDefaultAsync(t => t.ClerkOrgId == orgExternalId, cancellationToken);

        if (existing is not null)
        {
            return existing.Id;
        }

        var team = new Team
        {
            Id = Guid.NewGuid(),
            ClerkOrgId = orgExternalId,
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
            var raced = await _unitOfWork.Teams
                .FirstOrDefaultAsync(t => t.ClerkOrgId == orgExternalId, cancellationToken);
            return raced!.Id;
        }
    }

    private async Task SyncMembershipAsync(Guid userId, Guid teamId, string role, CancellationToken cancellationToken)
    {
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
        }
    }
}
