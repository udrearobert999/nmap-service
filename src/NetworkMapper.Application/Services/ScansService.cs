using NetworkMapper.Application.Mappers;
using NetworkMapper.Application.Services.Abstractions;
using NetworkMapper.Application.Validation;
using NetworkMapper.Contracts.Scans.Options;
using NetworkMapper.Contracts.Scans.Requests;
using NetworkMapper.Contracts.Scans.Responses;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Results.Generics;

namespace NetworkMapper.Application.Services;

internal sealed class ScansService : IScansService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationOrchestrator _validationOrchestrator;

    public ScansService(
        IUnitOfWork unitOfWork,
        IValidationOrchestrator validationOrchestrator)
    {
        _unitOfWork = unitOfWork;
        _validationOrchestrator = validationOrchestrator;
    }

    public async Task<Result<CreateScanResponseDto>> CreateAsync(
        IdempotentCreateScanRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationOrchestrator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result<CreateScanResponseDto>.ValidationFailure(validationResult.Error);
        }

        var alreadyProcessed = await _unitOfWork.IdempotentRequests
            .ExistsAsync(r => r.Id == request.RequestId, cancellationToken);

        if (alreadyProcessed)
        {
            var existingScan = await _unitOfWork.Scans
                .FirstOrDefaultAsync(s => s.RequestId == request.RequestId, cancellationToken);

            if (existingScan is not null)
            {
                return Result<CreateScanResponseDto>.Success(existingScan.ToCreateResponse());
            }
        }

        var scan = request.ToEntity();
        var idempotencyRecord = request.ToIdempotencyRecord();

        await _unitOfWork.Scans.CreateAsync(scan, cancellationToken);
        await _unitOfWork.IdempotentRequests.CreateAsync(idempotencyRecord, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateScanResponseDto>.Success(scan.ToCreateResponse());
    }

    public async Task<Result<GetScansResponseDto>> GetAllAsync(
        GetScansOptionsDto options,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationOrchestrator.ValidateAsync(options, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result<GetScansResponseDto>.ValidationFailure(validationResult.Error);
        }

        var scans = await _unitOfWork.Scans.GetScansAsync(options, cancellationToken);
        var scansCount = await _unitOfWork.Scans.CountAsync(
            s => options.Target == null || s.Target.Contains(options.Target),
            cancellationToken);

        return Result<GetScansResponseDto>.Success(scans.ToGetResponse(scansCount));
    }

    public async Task<Result<GetScanResponseDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var scan = await _unitOfWork.Scans.GetScanWithResultsByIdAsync(id, cancellationToken);

        if (scan is null)
        {
            return Result<GetScanResponseDto>.NotFound();
        }

        return Result<GetScanResponseDto>.Success(scan.ToGetResponse());
    }
}