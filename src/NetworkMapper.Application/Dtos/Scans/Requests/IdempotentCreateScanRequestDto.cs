using NetworkMapper.Application.Dtos.Abstractions;

namespace NetworkMapper.Application.Dtos.Scans.Requests;

public record IdempotentCreateScanRequestDto(
    string Target,
    Guid RequestId
) : IdempotentRequestDto(RequestId);