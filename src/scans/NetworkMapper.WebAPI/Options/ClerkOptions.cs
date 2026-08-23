namespace NetworkMapper.WebAPI.Options;

internal sealed class ClerkOptions
{
    public const string SectionName = "Clerk";

    public required string Authority { get; init; }
    public string? Audience { get; init; }
}
