using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vantage.Contracts.Constants;
using Vantage.Domain.Abstractions;

namespace Vantage.WebAPI.Controllers;

public record HealthResponse(string Database, IReadOnlyDictionary<string, string> Services);

[ApiController]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private static readonly TimeSpan HeartbeatFreshness = TimeSpan.FromSeconds(30);
    private const string Up = "up";
    private const string Down = "down";

    private readonly IServiceHeartbeatStore _heartbeatStore;
    private readonly DbContext _dbContext;

    public HealthController(IServiceHeartbeatStore heartbeatStore, DbContext dbContext)
    {
        _heartbeatStore = heartbeatStore;
        _dbContext = dbContext;
    }

    [HttpGet("/api/health")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var databaseUp = await _dbContext.Database.CanConnectAsync(cancellationToken);

        var heartbeats = databaseUp
            ? await _heartbeatStore.GetAllAsync(cancellationToken)
            : [];

        var now = DateTime.UtcNow;
        string StatusOf(string serviceName)
        {
            var heartbeat = heartbeats.FirstOrDefault(h => h.ServiceName == serviceName);
            return heartbeat is not null && now - heartbeat.LastSeenAt <= HeartbeatFreshness ? Up : Down;
        }

        var response = new HealthResponse(
            Database: databaseUp ? Up : Down,
            Services: new Dictionary<string, string>
            {
                [ServiceNames.Scanning] = StatusOf(ServiceNames.Scanning),
                [ServiceNames.Assessment] = StatusOf(ServiceNames.Assessment)
            });

        return Ok(response);
    }
}
