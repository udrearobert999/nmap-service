using MassTransit;
using NetworkMapper.Contracts.Scans.Messages;
using NetworkMapper.Domain.Entities;
using Newtonsoft.Json;

namespace NetworkMapper.Infrastructure.Outbox;

internal sealed class ScanRequestMessagePublisher : IOutboxMessagePublisher
{
    private readonly ITopicProducer<Guid, ScanRequestMessage> _producer;

    public ScanRequestMessagePublisher(ITopicProducer<Guid, ScanRequestMessage> producer)
    {
        _producer = producer;
    }

    public string MessageType => nameof(ScanRequestMessage);

    public async Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var payload = JsonConvert.DeserializeObject<ScanRequestMessage>(message.Message)
                      ?? throw new InvalidOperationException("Deserialization resulted in null payload.");

        await _producer.Produce(payload.ScanId, payload, cancellationToken);
    }
}
