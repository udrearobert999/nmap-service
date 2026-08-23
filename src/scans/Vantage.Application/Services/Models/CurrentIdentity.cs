namespace Vantage.Application.Services.Models;

public sealed record CurrentIdentity(
    Guid UserId,
    Guid? TeamId,
    string? Role);
