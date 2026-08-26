namespace Vantage.WebAPI.Options;

internal enum AuthMode
{
    DevHeaders,
    Clerk
}

internal sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public string? Mode { get; init; }

    public AuthMode Resolve(bool isDevelopment)
    {
        if (Enum.TryParse<AuthMode>(Mode, ignoreCase: true, out var parsed))
            return parsed;

        return isDevelopment ? AuthMode.DevHeaders : AuthMode.Clerk;
    }
}
