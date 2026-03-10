using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetworkMapper.Contracts.Scans.Messages;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Infrastructure.Persistence;
using Newtonsoft.Json;
using Quartz;

namespace NetworkMapper.Infrastructure.BackgroundJobs;

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
                var scopedProducer =
                    scope.ServiceProvider.GetRequiredService<ITopicProducer<Guid, ScanRequestMessage>>();

                await TryProcessSingleMessageAsync(message, scopedUnitOfWork, scopedProducer, token);
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
        ITopicProducer<Guid, ScanRequestMessage> scopedProducer,
        CancellationToken cancellationToken)
    {
        try
        {
            var payload = JsonConvert.DeserializeObject<ScanRequestMessage>(outboxMessage.Message);

            if (payload is null)
            {
                await scopedUnitOfWork.OutboxMessages.MarkAsFailedAsync(
                    outboxMessage.Id,
                    "Deserialization resulted in null payload.",
                    cancellationToken);
                return;
            }

            await scopedProducer.Produce(payload.ScanId, payload, cancellationToken);
            await scopedUnitOfWork.OutboxMessages.MarkAsCompletedAsync(outboxMessage.Id, cancellationToken);

            _logger.LogInformation(
                "Successfully dispatched outbox message {OutboxMessageId} to Kafka with key {ScanId}.",
                outboxMessage.Id,
                payload.ScanId);
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