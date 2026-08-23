namespace NetworkMapper.Domain.Abstractions;

public interface ICurrentTeamAccessor
{
    Guid? TeamId { get; }
}
