namespace NetworkMapper.Infrastructure.Options;

internal sealed class KafkaOptions
{
    public const string SectionName = "Kafka";
    public required string BootstrapServers { get; init; }
    public required string ScanRequestsTopic { get; init; }
}