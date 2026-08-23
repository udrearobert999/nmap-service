using Vantage.Domain.Abstractions;

namespace Vantage.WebAPI.Security;

public sealed class CurrentTeamAccessor : ICurrentTeamAccessor
{
    public Guid? TeamId { get; set; }
}
