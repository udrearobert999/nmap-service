using Vantage.Contracts.Scans.Options;
using Vantage.Contracts.Scans.Requests;
using Vantage.Contracts.Scans.Responses;
using Vantage.Domain.Results.Generics;

namespace Vantage.Application.Services.Abstractions;

public interface IScansService
{
    public Task<Result<CreateScanResponseDto>> CreateAsync(IdempotentCreateScanRequestDto request,
        CancellationToken cancellationToken = default);

    public Task<Result<GetScansResponseDto>> GetAllAsync(GetScansOptionsDto options,
        CancellationToken cancellationToken = default);

    public Task<Result<GetScanResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}