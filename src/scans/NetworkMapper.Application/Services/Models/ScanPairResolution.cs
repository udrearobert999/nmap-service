using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Application.Services.Models;

internal sealed record ScanPairResolution(
    Scan? OlderScan,
    Scan? NewerScan,
    string? Error)
{
    public bool HasError => Error is not null;
    public bool IsNotFound => OlderScan is null || NewerScan is null;
}