using NetworkMapper.Contracts.Scans;
using NetworkMapper.Contracts.Scans.Requests;
using NetworkMapper.Contracts.Scans.Responses;
using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Application.Mappers;

public static class ScanMapper
{
    public static ScanDto ToDto(this Scan scan) => 
        new(scan.Id, scan.Target, scan.Status, scan.CreatedAt);
    
    public static GetScanResponseDto ToGetResponse(this Scan scan) => 
        new(scan.Id, scan.Target, scan.Status, scan.CreatedAt);

    public static CreateScanResponseDto ToCreateResponse(this Scan scan) => 
        new(scan.Id, scan.Target, scan.Status, scan.CreatedAt);

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
        Status = "Pending",
        CreatedAt = DateTime.UtcNow
    };

    public static GetAllScansResponseDto ToGetAllResponse(this IEnumerable<Scan> scans, int totalCount)
    {
        return new GetAllScansResponseDto(
            scans.Select(s => s.ToDto()).ToList(), 
            totalCount
        );
    }
}