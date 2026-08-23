using NetworkMapper.Domain.Entities;

namespace NetworkMapper.Infrastructure.Outbox;

// One implementation per OutboxMessage.Type, so ProcessOutboxMessagesJob can
// dispatch a claimed message to the right Kafka producer without hard-coding
// a single message type.
internal interface IOutboxMessagePublisher
{
    string MessageType { get; }
    Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken);
}
