namespace Vantage.WebAPI.Options;

public sealed class ClerkWebhookOptions
{
    public const string SectionName = "ClerkWebhook";

    public string? SigningSecret { get; init; }
}
