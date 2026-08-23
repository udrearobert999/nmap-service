using NetworkMapper.Contracts.Abstractions;

namespace NetworkMapper.Contracts.Scans.Requests;

public record IdempotentCreateScanRequestDto(
    string Target,
    Guid RequestId
) : IdempotentRequestDto(RequestId);