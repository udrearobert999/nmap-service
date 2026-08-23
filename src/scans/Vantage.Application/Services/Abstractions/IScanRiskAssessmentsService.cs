using Vantage.Contracts.ScanRiskAssessments.Options;
using Vantage.Contracts.ScanRiskAssessments.Requests;
using Vantage.Contracts.ScanRiskAssessments.Responses;
using Vantage.Domain.Results.Generics;

namespace Vantage.Application.Services.Abstractions;

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
