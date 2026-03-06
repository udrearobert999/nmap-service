namespace NetworkMapper.Application.Dtos.Abstractions;

public abstract record IdempotentRequestDto(Guid RequestId);