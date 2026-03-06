namespace NetworkMapper.Contracts.Abstractions;

public interface IIdempotentRequest
{
    Guid RequestId { get; }
}