using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NetworkMapper.Contracts.Scans.Messages;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Infrastructure.Persistence.Mappers;
using Newtonsoft.Json;

namespace NetworkMapper.Infrastructure.Persistence.Interceptors;

internal sealed class CreateScanOutboxMessagesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;

        if (dbContext is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var outboxMessages = dbContext.ChangeTracker
            .Entries()
            .Where(x =>
                x is { Entity: Scan, State: EntityState.Added })
            .Select(x =>
            {
                var scan = (Scan)x.Entity;

                return new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    Type = nameof(ScanRequestMessage),
                    Message = JsonConvert.SerializeObject(scan.ToMessage(), new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    })
                };
            })
            .ToList();

        if (outboxMessages.Count != 0)
        {
            dbContext.Set<OutboxMessage>().AddRange(outboxMessages);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}