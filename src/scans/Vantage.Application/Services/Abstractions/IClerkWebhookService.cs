using System.Text.Json;

namespace Vantage.Application.Services.Abstractions;

public interface IClerkWebhookService
{
    Task<bool> HandleAsync(string eventType, JsonElement data, CancellationToken cancellationToken = default);
}
