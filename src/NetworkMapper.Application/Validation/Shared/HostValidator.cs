using System.Net;
using System.Net.Sockets;

namespace NetworkMapper.Application.Validation.Shared;

internal static class HostValidator
{
    public static bool IsValidHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return false;

        return IsValidIpAddress(host) || IsValidDnsName(host);
    }

    private static bool IsValidIpAddress(string host)
    {
        if (!IPAddress.TryParse(host, out var ipAddress))
            return false;

        return ipAddress.AddressFamily switch
        {
            AddressFamily.InterNetwork => host.AsSpan().Count('.') == 3,
            AddressFamily.InterNetworkV6 => true,
            _ => false
        };
    }

    private static bool IsValidDnsName(string host)
    {
        return Uri.CheckHostName(host) == UriHostNameType.Dns &&
               (host.Contains('.') || host.Equals("localhost", StringComparison.OrdinalIgnoreCase));
    }
}