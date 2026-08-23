namespace NetworkMapper.Domain.Entities.Abstractions;

public interface IEntity<TKey> where TKey : struct
{
    TKey Id { get; init; }
}