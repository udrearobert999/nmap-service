namespace NetworkMapper.Contracts.Abstractions;

public abstract record IdempotentRequestDto(Guid RequestId);