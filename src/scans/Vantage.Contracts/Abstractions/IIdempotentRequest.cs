namespace Vantage.Contracts.Abstractions;

public interface IIdempotentRequest
{
    Guid RequestId { get; }
}