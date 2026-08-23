using NetworkMapper.Contracts.Abstractions;

namespace NetworkMapper.Contracts.ScanRiskAssessments.Requests;

public record IdempotentCreateScanRiskAssessmentRequestDto(
    Guid ScanId,
    Guid RequestId
) : IdempotentRequestDto(RequestId);
