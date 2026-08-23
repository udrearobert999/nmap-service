using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Application.Services.Models;

internal readonly record struct PortKey(int Port, string Protocol)
{
    public static PortKey From(ScanResult result) =>
        new(result.Port, NormalizeProtocol(result.Protocol));

    private static string NormalizeProtocol(string protocol) =>
        protocol.Trim().ToUpperInvariant();
}