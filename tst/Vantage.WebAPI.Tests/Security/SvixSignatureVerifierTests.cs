using System;
using System.Security.Cryptography;
using System.Text;
using Vantage.WebAPI.Security;
using Xunit;

namespace Vantage.WebAPI.Tests.Security;

public class SvixSignatureVerifierTests
{
    private const string Secret = "whsec_c2VjcmV0LWtleS1mb3ItdGVzdGluZy1vbmx5";
    private const string Payload = "{\"type\":\"user.created\",\"data\":{\"id\":\"user_1\"}}";
    private const string Id = "msg_2b1c";

    private static readonly DateTimeOffset Now = DateTimeOffset.FromUnixTimeSeconds(1_700_000_000);

    private static string Sign(string id, string timestamp, string payload, string secret = Secret)
    {
        var raw = Convert.FromBase64String(secret["whsec_".Length..]);
        using var hmac = new HMACSHA256(raw);
        var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{id}.{timestamp}.{payload}"));

        return $"v1,{Convert.ToBase64String(signature)}";
    }

    private static string Timestamp(DateTimeOffset moment) =>
        moment.ToUnixTimeSeconds().ToString();

    [Fact]
    public void Verify_ShouldAccept_ValidSignature()
    {
        var timestamp = Timestamp(Now);

        var verified = SvixSignatureVerifier.Verify(
            Secret, Id, timestamp, Sign(Id, timestamp, Payload), Payload, Now);

        Assert.True(verified);
    }

    [Fact]
    public void Verify_ShouldAccept_WhenOneOfSeveralSignaturesMatches()
    {
        var timestamp = Timestamp(Now);
        var header = $"v1,{Convert.ToBase64String(new byte[32])} {Sign(Id, timestamp, Payload)}";

        var verified = SvixSignatureVerifier.Verify(Secret, Id, timestamp, header, Payload, Now);

        Assert.True(verified);
    }

    [Fact]
    public void Verify_ShouldReject_TamperedPayload()
    {
        var timestamp = Timestamp(Now);
        var header = Sign(Id, timestamp, Payload);

        var verified = SvixSignatureVerifier.Verify(
            Secret, Id, timestamp, header, Payload.Replace("user_1", "user_2"), Now);

        Assert.False(verified);
    }

    [Fact]
    public void Verify_ShouldReject_SignatureFromAnotherSecret()
    {
        var timestamp = Timestamp(Now);
        var header = Sign(Id, timestamp, Payload, "whsec_YW5vdGhlci1zZWNyZXQtZW50aXJlbHktaGVyZQ==");

        var verified = SvixSignatureVerifier.Verify(Secret, Id, timestamp, header, Payload, Now);

        Assert.False(verified);
    }

    [Fact]
    public void Verify_ShouldReject_ReplayedTimestamp()
    {
        var timestamp = Timestamp(Now.AddMinutes(-10));
        var header = Sign(Id, timestamp, Payload);

        var verified = SvixSignatureVerifier.Verify(Secret, Id, timestamp, header, Payload, Now);

        Assert.False(verified);
    }

    [Fact]
    public void Verify_ShouldReject_TimestampTooFarInTheFuture()
    {
        var timestamp = Timestamp(Now.AddMinutes(10));
        var header = Sign(Id, timestamp, Payload);

        var verified = SvixSignatureVerifier.Verify(Secret, Id, timestamp, header, Payload, Now);

        Assert.False(verified);
    }

    [Fact]
    public void Verify_ShouldReject_MismatchedMessageId()
    {
        var timestamp = Timestamp(Now);
        var header = Sign(Id, timestamp, Payload);

        var verified = SvixSignatureVerifier.Verify(Secret, "msg_other", timestamp, header, Payload, Now);

        Assert.False(verified);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Verify_ShouldReject_WhenSecretIsMissing(string? secret)
    {
        var timestamp = Timestamp(Now);

        var verified = SvixSignatureVerifier.Verify(
            secret, Id, timestamp, Sign(Id, timestamp, Payload), Payload, Now);

        Assert.False(verified);
    }

    [Fact]
    public void Verify_ShouldReject_WhenHeadersAreMissing()
    {
        var timestamp = Timestamp(Now);

        Assert.False(SvixSignatureVerifier.Verify(Secret, null, timestamp, "v1,x", Payload, Now));
        Assert.False(SvixSignatureVerifier.Verify(Secret, Id, null, "v1,x", Payload, Now));
        Assert.False(SvixSignatureVerifier.Verify(Secret, Id, timestamp, null, Payload, Now));
    }

    [Fact]
    public void Verify_ShouldReject_UnknownSignatureVersion()
    {
        var timestamp = Timestamp(Now);
        var header = Sign(Id, timestamp, Payload).Replace("v1,", "v2,");

        var verified = SvixSignatureVerifier.Verify(Secret, Id, timestamp, header, Payload, Now);

        Assert.False(verified);
    }

    [Fact]
    public void Verify_ShouldReject_MalformedHeaders()
    {
        var timestamp = Timestamp(Now);

        Assert.False(SvixSignatureVerifier.Verify(Secret, Id, timestamp, "garbage", Payload, Now));
        Assert.False(SvixSignatureVerifier.Verify(Secret, Id, timestamp, "v1,not-base64!!", Payload, Now));
        Assert.False(SvixSignatureVerifier.Verify(Secret, Id, "not-a-number", "v1,x", Payload, Now));
    }
}
