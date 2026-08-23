namespace Vantage.Domain.Abstractions;

public interface ICurrentUserAccessor
{
    Guid? UserId { get; }
    string? Role { get; }
}
