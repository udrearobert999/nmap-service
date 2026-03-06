using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetworkMapper.Infrastructure.Persistence;
using Quartz;

namespace NetworkMapper.Infrastructure.BackgroundJobs;

internal sealed class ProcessOutboxMessagesJob : IJob
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ProcessOutboxMessagesJob> _logger;

    public ProcessOutboxMessagesJob(AppDbContext dbContext,  ILogger<ProcessOutboxMessagesJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var messages = await _dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(20)
            .ToListAsync(context.CancellationToken);

        if (messages.Count == 0) return;

        foreach (var outboxMessage in messages)
        {
            try
            {
                _logger.LogInformation(
                    "Processing outbox message {@Id} of type {@Type}. Content: {@Message}",
                    outboxMessage.Id,
                    outboxMessage.Type,
                    outboxMessage.Message);

                outboxMessage.ProcessedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Failed to process outbox message {@Id}", 
                    outboxMessage.Id);
                
                outboxMessage.ErrorMessage = ex.Message;
            }
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}