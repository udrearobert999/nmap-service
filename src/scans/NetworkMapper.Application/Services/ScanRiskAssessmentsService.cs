using NetworkMapper.Application.Mappers;
using NetworkMapper.Application.Services.Abstractions;
using NetworkMapper.Application.Validation;
using NetworkMapper.Contracts.ScanRiskAssessments.Options;
using NetworkMapper.Contracts.ScanRiskAssessments.Requests;
using NetworkMapper.Contracts.ScanRiskAssessments.Responses;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Domain.Results.Generics;

namespace NetworkMapper.Application.Services;

internal sealed class ScanRiskAssessmentsService : IScanRiskAssessmentsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationOrchestrator _validationOrchestrator;
    private readonly ICurrentTeamAccessor _currentTeamAccessor;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public ScanRiskAssessmentsService(
        IUnitOfWork unitOfWork,
        IValidationOrchestrator validationOrchestrator,
        ICurrentTeamAccessor currentTeamAccessor,
        ICurrentUserAccessor currentUserAccessor)
    {
        _unitOfWork = unitOfWork;
        _validationOrchestrator = validationOrchestrator;
        _currentTeamAccessor = currentTeamAccessor;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<CreateScanRiskAssessmentResponseDto>> CreateAsync(
        IdempotentCreateScanRiskAssessmentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (_currentTeamAccessor.TeamId is not { } teamId || _currentUserAccessor.UserId is not { } userId)
        {
            return Result<CreateScanRiskAssessmentResponseDto>.ValidationFailure("Unable to resolve the current team.");
        }

        var validationResult = await _validationOrchestrator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result<CreateScanRiskAssessmentResponseDto>.ValidationFailure(validationResult.Error);
        }

        var existingScanRiskAssessment = await _unitOfWork.ScanRiskAssessments
            .FirstOrDefaultAsync(r => r.RequestId == request.RequestId, cancellationToken);

        if (existingScanRiskAssessment is not null)
        {
            return Result<CreateScanRiskAssessmentResponseDto>.Success(existingScanRiskAssessment.ToCreateResponse());
        }

        var scan = await _unitOfWork.Scans.GetScanWithResultsByIdAsync(request.ScanId, cancellationToken);
        if (scan is null)
        {
            return Result<CreateScanRiskAssessmentResponseDto>.NotFound();
        }

        var riskAssessment = request.ToEntity(teamId, userId);
        var outboxMessage = riskAssessment.ToOutboxMessage(scan);

        await _unitOfWork.ScanRiskAssessments.CreateAsync(riskAssessment, cancellationToken);
        await _unitOfWork.OutboxMessages.CreateAsync(outboxMessage, cancellationToken);

        return await ConcurrencySafeSaveAsync(riskAssessment, request.RequestId, cancellationToken);
    }

    private async Task<Result<CreateScanRiskAssessmentResponseDto>> ConcurrencySafeSaveAsync(
        ScanRiskAssessment riskAssessment,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<CreateScanRiskAssessmentResponseDto>.Success(riskAssessment.ToCreateResponse());
        }
        catch (Exception ex) when (_unitOfWork.IsUniqueConstraintViolation(ex))
        {
            var existingScanRiskAssessment = await _unitOfWork.ScanRiskAssessments
                .FirstOrDefaultAsync(r => r.RequestId == requestId, cancellationToken);

            return Result<CreateScanRiskAssessmentResponseDto>.Success(existingScanRiskAssessment!.ToCreateResponse());
        }
    }

    public async Task<Result<GetScanRiskAssessmentResponseDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var riskAssessment = await _unitOfWork.ScanRiskAssessments.GetWithFindingsByIdAsync(id, cancellationToken);

        if (riskAssessment is null)
        {
            return Result<GetScanRiskAssessmentResponseDto>.NotFound();
        }

        return Result<GetScanRiskAssessmentResponseDto>.Success(riskAssessment.ToGetResponse());
    }

    public async Task<Result<GetScanRiskAssessmentsResponseDto>> GetAllAsync(
        GetScanRiskAssessmentsOptionsDto options,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationOrchestrator.ValidateAsync(options, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result<GetScanRiskAssessmentsResponseDto>.ValidationFailure(validationResult.Error);
        }

        var riskAssessments = await _unitOfWork.ScanRiskAssessments.GetPagedAsync(options, cancellationToken);
        var totalCount = await _unitOfWork.ScanRiskAssessments.CountAsync(
            r => (options.Status == null || r.Status == options.Status) &&
                 (options.ScanId == null || r.ScanId == options.ScanId),
            cancellationToken);

        return Result<GetScanRiskAssessmentsResponseDto>.Success(riskAssessments.ToGetResponse(totalCount));
    }

    public Task<Result<GetScanRiskAssessmentsResponseDto>> GetByScanAsync(
        Guid scanId,
        GetScanRiskAssessmentsOptionsDto options,
        CancellationToken cancellationToken = default)
    {
        var scopedOptions = options with { ScanId = scanId };
        return GetAllAsync(scopedOptions, cancellationToken);
    }

    public async Task<Result<GetScanRiskAssessmentResponseDto>> GetLatestForScanAsync(
        Guid scanId,
        CancellationToken cancellationToken = default)
    {
        var riskAssessment = await _unitOfWork.ScanRiskAssessments.GetLatestByScanAsync(scanId, cancellationToken);

        if (riskAssessment is null)
        {
            return Result<GetScanRiskAssessmentResponseDto>.NotFound();
        }

        return Result<GetScanRiskAssessmentResponseDto>.Success(riskAssessment.ToGetResponse());
    }
}
