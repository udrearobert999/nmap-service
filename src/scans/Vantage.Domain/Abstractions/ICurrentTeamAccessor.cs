namespace Vantage.Domain.Abstractions;

public interface ICurrentTeamAccessor
{
    Guid? TeamId { get; }
}
