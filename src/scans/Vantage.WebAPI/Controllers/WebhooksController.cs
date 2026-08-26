using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Vantage.Application.Services.Abstractions;
using Vantage.WebAPI.Options;
using Vantage.WebAPI.Security;

namespace Vantage.WebAPI.Controllers;

[AllowAnonymous]
public class WebhooksController : ApiControllerBase
{
    private const int MaxPayloadBytes = 512 * 1024;

    private readonly IClerkWebhookService _clerkWebhookService;
    private readonly IOptions<ClerkWebhookOptions> _options;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IClerkWebhookService clerkWebhookService,
        IOptions<ClerkWebhookOptions> options,
        ILogger<WebhooksController> logger)
    {
        _clerkWebhookService = clerkWebhookService;
        _options = options;
        _logger = logger;
    }

    [HttpPost("clerk")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Clerk(CancellationToken cancellationToken)
    {
        var signingSecret = _options.Value.SigningSecret;
        if (string.IsNullOrWhiteSpace(signingSecret))
        {
            _logger.LogWarning("Clerk webhook received but ClerkWebhook:SigningSecret is not configured.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        if (Request.ContentLength > MaxPayloadBytes)
        {
            return BadRequest();
        }

        string payload;
        using (var reader = new StreamReader(Request.Body))
        {
            payload = await reader.ReadToEndAsync(cancellationToken);
        }

        var verified = SvixSignatureVerifier.Verify(
            signingSecret,
            Request.Headers["svix-id"].FirstOrDefault(),
            Request.Headers["svix-timestamp"].FirstOrDefault(),
            Request.Headers["svix-signature"].FirstOrDefault(),
            payload,
            DateTimeOffset.UtcNow);

        if (!verified)
        {
            _logger.LogWarning("Rejected a Clerk webhook with an invalid signature.");
            return Unauthorized();
        }

        JsonElement root;
        try
        {
            using var document = JsonDocument.Parse(payload);
            root = document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return BadRequest();
        }

        var eventType = root.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : null;
        if (string.IsNullOrWhiteSpace(eventType) || !root.TryGetProperty("data", out var data))
        {
            return BadRequest();
        }

        var handled = await _clerkWebhookService.HandleAsync(eventType, data, cancellationToken);

        _logger.LogInformation(
            "Clerk webhook {EventType} {Outcome}.", eventType, handled ? "applied" : "ignored");

        return Ok();
    }
}
