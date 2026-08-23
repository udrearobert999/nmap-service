using NetworkMapper.Contracts.ScanRiskAssessments.Options;
using NetworkMapper.Contracts.ScanRiskAssessments.Requests;
using NetworkMapper.Contracts.ScanRiskAssessments.Responses;
using NetworkMapper.Domain.Results.Generics;

namespace NetworkMapper.Application.Services.Abstractions;

public interface IScanRiskAssessmentsService
{
    public Task<Result<CreateScanRiskAssessmentResponseDto>> CreateAsync(
        IdempotentCreateScanRiskAssessmentRequestDto request,
        CancellationToken cancellationToken = default);

    public Task<Result<GetScanRiskAssessmentResponseDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    public Task<Result<GetScanRiskAssessmentsResponseDto>> GetAllAsync(
        GetScanRiskAssessmentsOptionsDto options,
        CancellationToken cancellationToken = default);

    public Task<Result<GetScanRiskAssessmentsResponseDto>> GetByScanAsync(
        Guid scanId,
        GetScanRiskAssessmentsOptionsDto options,
        CancellationToken cancellationToken = default);

    public Task<Result<GetScanRiskAssessmentResponseDto>> GetLatestForScanAsync(
        Guid scanId,
        CancellationToken cancellationToken = default);
}
