using System.Security.Cryptography;
using System.Text;

namespace Vantage.WebAPI.Security;

public static class SvixSignatureVerifier
{
    private const string SecretPrefix = "whsec_";
    private static readonly TimeSpan Tolerance = TimeSpan.FromMinutes(5);

    public static bool Verify(
        string? signingSecret,
        string? id,
        string? timestamp,
        string? signatureHeader,
        string payload,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(signingSecret) ||
            string.IsNullOrWhiteSpace(id) ||
            string.IsNullOrWhiteSpace(timestamp) ||
            string.IsNullOrWhiteSpace(signatureHeader))
        {
            return false;
        }

        if (!IsTimestampFresh(timestamp, now))
        {
            return false;
        }

        if (!TryDecodeSecret(signingSecret, out var secret))
        {
            return false;
        }

        var signedContent = $"{id}.{timestamp}.{payload}";

        using var hmac = new HMACSHA256(secret);
        var expected = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedContent));

        foreach (var candidate in signatureHeader.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = candidate.IndexOf(',');
            if (separator < 0)
            {
                continue;
            }

            var version = candidate[..separator];
            if (!string.Equals(version, "v1", StringComparison.Ordinal))
            {
                continue;
            }

            var encoded = candidate[(separator + 1)..];
            if (!TryDecodeBase64(encoded, out var provided))
            {
                continue;
            }

            if (CryptographicOperations.FixedTimeEquals(expected, provided))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsTimestampFresh(string timestamp, DateTimeOffset now)
    {
        if (!long.TryParse(timestamp, out var seconds))
        {
            return false;
        }

        var sent = DateTimeOffset.FromUnixTimeSeconds(seconds);
        var drift = now - sent;

        return drift <= Tolerance && drift >= -Tolerance;
    }

    private static bool TryDecodeSecret(string signingSecret, out byte[] secret)
    {
        var raw = signingSecret.StartsWith(SecretPrefix, StringComparison.Ordinal)
            ? signingSecret[SecretPrefix.Length..]
            : signingSecret;

        return TryDecodeBase64(raw, out secret);
    }

    private static bool TryDecodeBase64(string value, out byte[] decoded)
    {
        var buffer = new byte[((value.Length + 3) / 4) * 3];

        if (Convert.TryFromBase64String(value, buffer, out var written))
        {
            decoded = buffer[..written];
            return true;
        }

        decoded = [];
        return false;
    }
}
