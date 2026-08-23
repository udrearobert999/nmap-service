using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Vantage.WebAPI.Security;

public sealed class DevSubjectAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "DevSubject";

    private const string SubjectHeader = "X-Dev-Subject";
    private const string OrgHeader = "X-Dev-Org";
    private const string RoleHeader = "X-Dev-Role";
    private const string EmailHeader = "X-Dev-Email";

    private const string DefaultSubject = "dev|default-user";

    public DevSubjectAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var subject = Header(SubjectHeader) ?? DefaultSubject;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, subject),
            new("name", subject)
        };

        var email = Header(EmailHeader);
        if (email is not null)
        {
            claims.Add(new Claim("email", email));
        }

        var org = Header(OrgHeader);
        if (org is not null)
        {
            claims.Add(new Claim("org_id", org));
            claims.Add(new Claim("org_role", Header(RoleHeader) ?? "admin"));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private string? Header(string name) =>
        Request.Headers.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : null;
}
