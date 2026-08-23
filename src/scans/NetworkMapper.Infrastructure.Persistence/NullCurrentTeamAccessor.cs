using NetworkMapper.Domain.Abstractions;

namespace NetworkMapper.Infrastructure.Persistence;

internal sealed class NullCurrentTeamAccessor : ICurrentTeamAccessor
{
    public Guid? TeamId => null;
}
