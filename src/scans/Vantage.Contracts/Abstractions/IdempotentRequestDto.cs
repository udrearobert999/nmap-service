namespace Vantage.Contracts.Abstractions;

public abstract record IdempotentRequestDto(Guid RequestId);