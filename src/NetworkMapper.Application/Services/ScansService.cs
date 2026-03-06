using NetworkMapper.Application.Mappers;
using NetworkMapper.Application.Services.Abstractions;
using NetworkMapper.Application.Specifications;
using NetworkMapper.Application.Validation;
using NetworkMapper.Contracts.Scans.Options;
using NetworkMapper.Contracts.Scans.Requests;
using NetworkMapper.Contracts.Scans.Responses;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Results.Generics;

namespace NetworkMapper.Application.Services;

internal class ScansService : IScansService
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

    var existingIdempotencyRecord = await _unitOfWork.IdempotentRequests
        .GetByIdAsync(request.RequestId, cancellationToken);

    if (existingIdempotencyRecord is not null)
    {
        var spec = new GetScanByRequestIdSpec(request.RequestId);
        var existingScan = await _unitOfWork.Scans.GetSingleOrDefaultBySpecAsync(spec, cancellationToken);
        
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

    public async Task<Result<GetAllScansResponseDto>> GetAllPaginatedAsync(
        GetAllScansOptionsDto options,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationOrchestrator.ValidateAsync(options, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result<GetAllScansResponseDto>.ValidationFailure(validationResult.Error);
        }

        var getScansSpec = new GetAllScansWithResultsPagedSpec(options);
        var scans = await _unitOfWork.Scans.GetBySpecAsync(getScansSpec, cancellationToken);

        var countScansSpec = new CountScansByGetAllRequestSpec(options);
        var scansCount = await _unitOfWork.Scans.CountBySpecAsync(countScansSpec, cancellationToken);

        var responseDto = scans.ToGetAllResponse(scansCount);

        return Result<GetAllScansResponseDto>.Success(responseDto);
    }

    public async Task<Result<GetScanResponseDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var scan = await _unitOfWork.Scans.GetByIdAsync(id, cancellationToken, track: false);
        
        if (scan is null)
        {
            return Result<GetScanResponseDto>.NotFound();
        }

        return Result<GetScanResponseDto>.Success(scan.ToGetResponse());
    }
}