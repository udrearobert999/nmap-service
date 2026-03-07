using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetworkMapper.Contracts.Scans.Messages;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Infrastructure.Persistence;
using Newtonsoft.Json;
using Quartz;

namespace NetworkMapper.Infrastructure.BackgroundJobs;

internal sealed class ProcessOutboxMessagesJob : IJob
{
    private readonly AppDbContext _dbContext;
    private readonly ITopicProducer<ScanRequestMessage> _producer;
    private readonly ILogger<ProcessOutboxMessagesJob> _logger;

    public ProcessOutboxMessagesJob(
        AppDbContext dbContext,
        ITopicProducer<ScanRequestMessage> producer,
        ILogger<ProcessOutboxMessagesJob> logger)
    {
        _dbContext = dbContext;
        _producer = producer;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var outboxMessages = await GetUnprocessedMessagesAsync(context.CancellationToken);

        if (outboxMessages.Count == 0)
            return;

        await ProcessMessagesAsync(outboxMessages, context.CancellationToken);
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }

    private async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);
    }

    private async Task ProcessMessagesAsync(List<OutboxMessage> outboxMessages, CancellationToken cancellationToken)
    {
        foreach (var message in outboxMessages)
        {
            await TryProcessSingleMessageAsync(message, cancellationToken);
        }
    }

    private async Task TryProcessSingleMessageAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        try
        {
            var payload = JsonConvert.DeserializeObject<ScanRequestMessage>(outboxMessage.Message);

            if (payload is null)
            {
                outboxMessage.ErrorMessage = "Deserialization resulted in null payload.";
                return;
            }

            await _producer.Produce(payload, cancellationToken);

            outboxMessage.ProcessedAt = DateTime.UtcNow;
            _logger.LogInformation("Successfully dispatched outbox message {Id} to Kafka.", outboxMessage.Id);
        }
        catch (Exception ex)
        {
            outboxMessage.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Failed to process outbox message {Id}", outboxMessage.Id);
        }
    }
}