using NetworkMapper.Contracts.Constants;
using NetworkMapper.Contracts.Scans;
using NetworkMapper.Contracts.Scans.Messages;
using NetworkMapper.Contracts.Scans.Options;
using NetworkMapper.Contracts.Scans.Requests;
using NetworkMapper.Contracts.Scans.Responses;
using NetworkMapper.Domain.Entities;
using Newtonsoft.Json;

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

    public static Scan ToEntity(this IdempotentCreateScanRequestDto dto) => new()
    {
        Id = Guid.NewGuid(),
        RequestId = dto.RequestId,
        Target = dto.Target,
        Status = Status.Pending,
        CreatedAt = DateTime.UtcNow
    };

    public static ScanRequestMessage ToMessage(this Scan scan)
        => new(scan.Id, scan.Target);

    public static OutboxMessage ToOutboxMessage(this Scan scan) => new()
    {
        Id = Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow,
        Type = nameof(ScanRequestMessage),
        Status = Status.Pending,
        Message = JsonConvert.SerializeObject(
            scan.ToMessage(),
            new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            })
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