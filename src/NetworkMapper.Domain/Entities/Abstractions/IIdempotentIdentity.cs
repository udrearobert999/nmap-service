namespace NetworkMapper.Domain.Entities.Abstractions;

public interface IIdempotentEntity
{
    Guid RequestId { get; init; }
}