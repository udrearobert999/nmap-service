using NetworkMapper.Domain.Abstractions;

namespace NetworkMapper.WebAPI.Security;

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    public Guid? UserId { get; set; }
    public string? Role { get; set; }
}
