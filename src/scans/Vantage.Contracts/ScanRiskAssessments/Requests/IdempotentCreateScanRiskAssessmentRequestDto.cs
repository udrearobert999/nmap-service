using Vantage.Contracts.Abstractions;

namespace Vantage.Contracts.ScanRiskAssessments.Requests;

public record IdempotentCreateScanRiskAssessmentRequestDto(
    Guid ScanId,
    Guid RequestId
) : IdempotentRequestDto(RequestId);
