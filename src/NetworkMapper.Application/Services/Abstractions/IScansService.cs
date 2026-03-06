using NetworkMapper.Application.Dtos.Scans.Options;
using NetworkMapper.Application.Dtos.Scans.Requests;
using NetworkMapper.Application.Dtos.Scans.Responses;
using NetworkMapper.Domain.Results.Generics;

namespace NetworkMapper.Application.Services.Abstractions;

public interface IScansService
{
    public Task<Result<CreateScanResponseDto>> CreateAsync(IdempotentCreateScanRequestDto request,
        CancellationToken cancellationToken = default);

    public Task<Result<GetAllScansResponseDto>> GetAllPaginatedAsync(GetAllScansOptionsDto options,
        CancellationToken cancellationToken = default);

    public Task<Result<GetScanResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}