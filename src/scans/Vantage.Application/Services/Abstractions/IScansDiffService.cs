using Vantage.Contracts.Scans.Requests;
using Vantage.Contracts.Scans.Responses;
using Vantage.Domain.Results.Generics;

namespace Vantage.Application.Services.Abstractions;

public interface IScansDiffService
{
    Task<Result<GetScansDiffResponseDto>> GetDiffAsync(GetScansDiffRequestDto request,
        CancellationToken cancellationToken = default);
}