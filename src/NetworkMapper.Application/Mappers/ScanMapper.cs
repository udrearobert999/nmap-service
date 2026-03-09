using NetworkMapper.Contracts.Constants;
using NetworkMapper.Contracts.Scans;
using NetworkMapper.Contracts.Scans.Options;
using NetworkMapper.Contracts.Scans.Requests;
using NetworkMapper.Contracts.Scans.Responses;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Application.Mappers;

public static class ScanMapper
{
    public static ScanResultDto ToDto(this ScanResult result) =>
        new(result.Port, result.Protocol, result.Service, result.State);

    public static ScanDto ToDto(this Scan scan) =>
        new(
            scan.Id,
            scan.Target,
            scan.Status,
            scan.CreatedAt,
            scan.CompletedAt,
            scan.ErrorMessage,
            scan.Results.Select(r => r.ToDto()).ToList()
        );

    public static GetScanResponseDto ToGetResponse(this Scan scan) =>
        new(
            scan.Id,
            scan.Target,
            scan.Status,
            scan.CreatedAt,
            scan.CompletedAt,
            scan.ErrorMessage,
            scan.Results.Select(r => r.ToDto()).ToList()
        );

    public static CreateScanResponseDto ToCreateResponse(this Scan scan) =>
        new(scan.Id, scan.Target, scan.Status, scan.CreatedAt, scan.CompletedAt);

    public static IdempotentCreateScanRequestDto ToIdempotentRequest(
        this CreateScanRequestDto dto,
        Guid idempotencyKey)
        => new(dto.Target, idempotencyKey);

    public static IdempotentRequest ToIdempotencyRecord(this IdempotentCreateScanRequestDto request) => new()
    {
        Id = request.RequestId,
        Name = nameof(IdempotentCreateScanRequestDto),
        CreatedAt = DateTime.UtcNow
    };

    public static Scan ToEntity(this IdempotentCreateScanRequestDto dto) => new()
    {
        Id = Guid.NewGuid(),
        RequestId = dto.RequestId,
        Target = dto.Target,
        Status = ScanStatus.Pending,
        CreatedAt = DateTime.UtcNow
    };

    public static GetScansResponseDto ToGetResponse(this IEnumerable<Scan> scans, int totalCount)
    {
        return new GetScansResponseDto(
            scans.Select(s => s.ToDto()).ToList(),
            totalCount
        );
    }

    public static GetScansDiffRequestDto ToRequest(this GetScansDiffOptions options, string target) =>
        new(target, options.From, options.To);
}