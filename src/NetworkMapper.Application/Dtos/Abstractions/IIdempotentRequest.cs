namespace NetworkMapper.Application.Dtos.Abstractions;

public interface IIdempotentRequest
{
    Guid RequestId { get; }
}