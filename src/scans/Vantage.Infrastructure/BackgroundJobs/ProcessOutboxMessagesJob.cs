using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vantage.Domain.Abstractions;
using Vantage.Domain.Entities;
using Vantage.Infrastructure.Outbox;
using Quartz;

namespace Vantage.Infrastructure.BackgroundJobs;

internal sealed class ProcessOutboxMessagesJob : IJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProcessOutboxMessagesJob> _logger;

    public ProcessOutboxMessagesJob(
        IUnitOfWork unitOfWork,
        IServiceScopeFactory scopeFactory,
        ILogger<ProcessOutboxMessagesJob> logger)
    {
        _unitOfWork = unitOfWork;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var outboxMessages = await _unitOfWork.OutboxMessages.ClaimScanAsync(20, context.CancellationToken);
        if (outboxMessages.Count == 0)
            return;

        await Parallel.ForEachAsync(outboxMessages, GetParallelOptions(context.CancellationToken),
            async (message, token) =>
            {
                await using var scope = _scopeFactory.CreateAsyncScope();

                var scopedUnitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var publishers = scope.ServiceProvider.GetServices<IOutboxMessagePublisher>();

                await TryProcessSingleMessageAsync(message, scopedUnitOfWork, publishers, token);
            });
    }

    private static ParallelOptions GetParallelOptions(CancellationToken cancellationToken)
    {
        return new ParallelOptions
        {
            MaxDegreeOfParallelism = 5,
            CancellationToken = cancellationToken
        };
    }

    private async Task TryProcessSingleMessageAsync(
        OutboxMessage outboxMessage,
        IUnitOfWork scopedUnitOfWork,
        IEnumerable<IOutboxMessagePublisher> publishers,
        CancellationToken cancellationToken)
    {
        try
        {
            var publisher = publishers.FirstOrDefault(p => p.MessageType == outboxMessage.Type);
            if (publisher is null)
            {
                await scopedUnitOfWork.OutboxMessages.MarkAsFailedAsync(
                    outboxMessage.Id,
                    $"No publisher registered for outbox message type '{outboxMessage.Type}'.",
                    cancellationToken);
                return;
            }

            await publisher.PublishAsync(outboxMessage, cancellationToken);
            await scopedUnitOfWork.OutboxMessages.MarkAsCompletedAsync(outboxMessage.Id, cancellationToken);

            _logger.LogInformation(
                "Successfully dispatched outbox message {OutboxMessageId} of type {Type} to Kafka.",
                outboxMessage.Id,
                outboxMessage.Type);
        }
        catch (Exception ex)
        {
            await scopedUnitOfWork.OutboxMessages.MarkAsFailedAsync(outboxMessage.Id, ex.Message, cancellationToken);
            _logger.LogError(
                ex,
                "Failed to process outbox message {OutboxMessageId}",
                outboxMessage.Id);
        }
    }
}
