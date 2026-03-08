using NetworkMapper.Contracts.Scans.Options;
using NetworkMapper.Contracts.Scans.Requests;
using NetworkMapper.Contracts.Scans.Responses;
using NetworkMapper.Domain.Results.Generics;

namespace NetworkMapper.Application.Services.Abstractions;

public interface IScansService
{
    public Task<Result<CreateScanResponseDto>> CreateAsync(IdempotentCreateScanRequestDto request,
        CancellationToken cancellationToken = default);

    public Task<Result<GetScansResponseDto>> GetAllAsync(GetScansOptionsDto options,
        CancellationToken cancellationToken = default);

    public Task<Result<GetScanResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}