using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Vantage.Application.Services;
using Vantage.Domain.Abstractions;
using Vantage.Domain.Entities;
using Xunit;

namespace Vantage.WebAPI.Tests.Services;

public class ClerkWebhookServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IRepository<User, Guid>> _usersMock = new();
    private readonly Mock<IRepository<Team, Guid>> _teamsMock = new();
    private readonly Mock<IRepository<TeamMembership, Guid>> _membershipsMock = new();
    private readonly ClerkWebhookService _sut;

    public ClerkWebhookServiceTests()
    {
        _unitOfWorkMock.SetupGet(u => u.Users).Returns(_usersMock.Object);
        _unitOfWorkMock.SetupGet(u => u.Teams).Returns(_teamsMock.Object);
        _unitOfWorkMock.SetupGet(u => u.TeamMemberships).Returns(_membershipsMock.Object);

        _sut = new ClerkWebhookService(_unitOfWorkMock.Object);
    }

    private static JsonElement Json(string raw) => JsonDocument.Parse(raw).RootElement.Clone();

    private void UserExists(User? user) =>
        _usersMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(user);

    private void TeamExists(Team? team) =>
        _teamsMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Team, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(team);

    private void MembershipExists(TeamMembership? membership) =>
        _membershipsMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<TeamMembership, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(membership);

    [Fact]
    public async Task HandleAsync_ShouldIgnore_UnknownEventType()
    {
        var handled = await _sut.HandleAsync("session.created", Json("{}"));

        Assert.False(handled);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateUser_OnUserCreated()
    {
        UserExists(null);
        User? created = null;
        _usersMock
            .Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => created = u)
            .ReturnsAsync((User u, CancellationToken _) => u);

        var handled = await _sut.HandleAsync("user.created", Json("""
        {
          "id": "user_1",
          "first_name": "Ada",
          "last_name": "Lovelace",
          "primary_email_address_id": "idn_2",
          "email_addresses": [
            { "id": "idn_1", "email_address": "old@example.com" },
            { "id": "idn_2", "email_address": "ada@example.com" }
          ]
        }
        """));

        Assert.True(handled);
        Assert.NotNull(created);
        Assert.Equal("user_1", created!.ClerkUserId);
        Assert.Equal("ada@example.com", created.Email);
        Assert.Equal("Ada Lovelace", created.Name);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateExistingUser_OnUserUpdated()
    {
        var existing = new User
        {
            Id = Guid.NewGuid(),
            ClerkUserId = "user_1",
            Email = "old@example.com",
            Name = "Old Name",
            CreatedAt = DateTime.UtcNow
        };
        UserExists(existing);

        await _sut.HandleAsync("user.updated", Json("""
        {
          "id": "user_1",
          "first_name": "New",
          "last_name": "Name",
          "email_addresses": [{ "id": "idn_1", "email_address": "new@example.com" }]
        }
        """));

        Assert.Equal("new@example.com", existing.Email);
        Assert.Equal("New Name", existing.Name);
        _usersMock.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldRevokeMembershipsButKeepUser_OnUserDeleted()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            ClerkUserId = "user_1",
            CreatedAt = DateTime.UtcNow
        };
        UserExists(user);

        var memberships = new List<TeamMembership>
        {
            new() { Id = Guid.NewGuid(), UserId = user.Id, TeamId = Guid.NewGuid(), Role = "admin" },
            new() { Id = Guid.NewGuid(), UserId = user.Id, TeamId = Guid.NewGuid(), Role = "member" }
        };

        _membershipsMock
            .Setup(r => r.GetByExpressionAsync(
                It.IsAny<Expression<Func<TeamMembership, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(memberships);

        var handled = await _sut.HandleAsync("user.deleted", Json("""{ "id": "user_1", "deleted": true }"""));

        Assert.True(handled);
        _membershipsMock.Verify(
            r => r.DeleteAsync(It.IsAny<TeamMembership>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _usersMock.Verify(r => r.DeleteAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldDeleteTeam_OnOrganizationDeleted()
    {
        var team = new Team
        {
            Id = Guid.NewGuid(),
            ClerkOrgId = "org_1",
            CreatedAt = DateTime.UtcNow
        };
        TeamExists(team);

        var handled = await _sut.HandleAsync("organization.deleted", Json("""{ "id": "org_1" }"""));

        Assert.True(handled);
        _teamsMock.Verify(r => r.DeleteAsync(team, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateMembershipAndStripRolePrefix_OnMembershipCreated()
    {
        TeamExists(new Team { Id = Guid.NewGuid(), ClerkOrgId = "org_1", CreatedAt = DateTime.UtcNow });
        UserExists(new User { Id = Guid.NewGuid(), ClerkUserId = "user_1", CreatedAt = DateTime.UtcNow });
        MembershipExists(null);

        TeamMembership? created = null;
        _membershipsMock
            .Setup(r => r.CreateAsync(It.IsAny<TeamMembership>(), It.IsAny<CancellationToken>()))
            .Callback<TeamMembership, CancellationToken>((m, _) => created = m)
            .ReturnsAsync((TeamMembership m, CancellationToken _) => m);

        var handled = await _sut.HandleAsync("organizationMembership.created", Json("""
        {
          "role": "org:admin",
          "organization": { "id": "org_1", "name": "Acme" },
          "public_user_data": { "user_id": "user_1", "first_name": "Ada", "last_name": "Lovelace" }
        }
        """));

        Assert.True(handled);
        Assert.NotNull(created);
        Assert.Equal("admin", created!.Role);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateRole_OnMembershipUpdated()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        TeamExists(new Team { Id = teamId, ClerkOrgId = "org_1", CreatedAt = DateTime.UtcNow });
        UserExists(new User { Id = userId, ClerkUserId = "user_1", CreatedAt = DateTime.UtcNow });

        var membership = new TeamMembership
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TeamId = teamId,
            Role = "member"
        };
        MembershipExists(membership);

        await _sut.HandleAsync("organizationMembership.updated", Json("""
        {
          "role": "org:admin",
          "organization": { "id": "org_1" },
          "public_user_data": { "user_id": "user_1" }
        }
        """));

        Assert.Equal("admin", membership.Role);
        _membershipsMock.Verify(
            r => r.CreateAsync(It.IsAny<TeamMembership>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldRemoveMembership_OnMembershipDeleted()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        TeamExists(new Team { Id = teamId, ClerkOrgId = "org_1", CreatedAt = DateTime.UtcNow });
        UserExists(new User { Id = userId, ClerkUserId = "user_1", CreatedAt = DateTime.UtcNow });

        var membership = new TeamMembership
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TeamId = teamId,
            Role = "member"
        };
        MembershipExists(membership);

        var handled = await _sut.HandleAsync("organizationMembership.deleted", Json("""
        {
          "organization": { "id": "org_1" },
          "public_user_data": { "user_id": "user_1" }
        }
        """));

        Assert.True(handled);
        _membershipsMock.Verify(r => r.DeleteAsync(membership, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldIgnore_MembershipEventMissingIdentifiers()
    {
        var handled = await _sut.HandleAsync("organizationMembership.deleted", Json("""{ "organization": {} }"""));

        Assert.True(handled);
        _membershipsMock.Verify(
            r => r.DeleteAsync(It.IsAny<TeamMembership>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
