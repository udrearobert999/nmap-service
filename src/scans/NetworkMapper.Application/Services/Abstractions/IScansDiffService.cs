using NetworkMapper.Contracts.Scans.Requests;
using NetworkMapper.Contracts.Scans.Responses;
using NetworkMapper.Domain.Results.Generics;

namespace NetworkMapper.Application.Services.Abstractions;

public interface IScansDiffService
{
    Task<Result<GetScansDiffResponseDto>> GetDiffAsync(GetScansDiffRequestDto request,
        CancellationToken cancellationToken = default);
}