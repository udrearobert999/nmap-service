using Vantage.Contracts.Abstractions;

namespace Vantage.Contracts.Scans.Requests;

public record IdempotentCreateScanRequestDto(
    string Target,
    Guid RequestId
) : IdempotentRequestDto(RequestId);