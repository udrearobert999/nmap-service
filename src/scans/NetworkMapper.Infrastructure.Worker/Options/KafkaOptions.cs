namespace NetworkMapper.Infrastructure.Worker.Options;

internal sealed class KafkaOptions
{
    public const string SectionName = "Kafka";
    public required string BootstrapServers { get; init; }
    public required string ScanRequestsTopic { get; init; }
    public required string ScanRequestsConsumerGroup { get; init; }
    public required int ScanRequestsConcurrentMessagesLimit { get; init; }
    public required ushort ScanRequestsConcurrentConsumerLimit { get; init; }
}