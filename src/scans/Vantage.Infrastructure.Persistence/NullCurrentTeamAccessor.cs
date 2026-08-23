using Vantage.Domain.Abstractions;

namespace Vantage.Infrastructure.Persistence;

internal sealed class NullCurrentTeamAccessor : ICurrentTeamAccessor
{
    public Guid? TeamId => null;
}
