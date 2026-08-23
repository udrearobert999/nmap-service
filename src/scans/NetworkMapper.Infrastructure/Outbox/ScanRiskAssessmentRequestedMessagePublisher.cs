using Confluent.Kafka;
using Microsoft.Extensions.Options;
using NetworkMapper.Contracts.ScanRiskAssessments.Messages;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Infrastructure.Options;
using Newtonsoft.Json;

namespace NetworkMapper.Infrastructure.Outbox;

internal sealed class ScanRiskAssessmentRequestedMessagePublisher : IOutboxMessagePublisher
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;

    public ScanRiskAssessmentRequestedMessagePublisher(
        IProducer<string, string> producer,
        IOptions<KafkaOptions> kafkaOptions)
    {
        _producer = producer;
        _topic = kafkaOptions.Value.ScanRiskAssessmentRequestsTopic;
    }

    public string MessageType => nameof(ScanRiskAssessmentRequestedMessage);

    public async Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var payload = JsonConvert.DeserializeObject<ScanRiskAssessmentRequestedMessage>(message.Message)
                      ?? throw new InvalidOperationException("Deserialization resulted in null payload.");

        await _producer.ProduceAsync(
            _topic,
            new Message<string, string>
            {
                Key = payload.ScanRiskAssessmentId.ToString(),
                Value = message.Message
            },
            cancellationToken);
    }
}
