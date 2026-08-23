using NetworkMapper.Domain.Abstractions;

namespace NetworkMapper.WebAPI.Security;

public sealed class CurrentTeamAccessor : ICurrentTeamAccessor
{
    public Guid? TeamId { get; set; }
}
