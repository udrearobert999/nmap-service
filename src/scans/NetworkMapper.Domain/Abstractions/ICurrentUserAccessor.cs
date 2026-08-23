namespace NetworkMapper.Domain.Abstractions;

public interface ICurrentUserAccessor
{
    Guid? UserId { get; }
    string? Role { get; }
}
