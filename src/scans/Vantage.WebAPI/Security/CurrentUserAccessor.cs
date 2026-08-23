using Vantage.Domain.Abstractions;

namespace Vantage.WebAPI.Security;

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    public Guid? UserId { get; set; }
    public string? Role { get; set; }
}
