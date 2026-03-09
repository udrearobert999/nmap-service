using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetworkMapper.Contracts.Scans.Messages;
using NetworkMapper.Infrastructure.Worker.Consumers;
using NetworkMapper.Infrastructure.Worker.Options;

namespace NetworkMapper.Infrastructure.Worker;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection(KafkaOptions.SectionName));

        services.AddMassTransit(x =>
        {
            x.UsingInMemory();
            x.AddRider(rider =>
            {
                rider.AddConsumer<ScanRequestConsumer>();
                rider.UsingKafka((context, k) =>
                {
                    var options = context.GetRequiredService<IOptions<KafkaOptions>>().Value;

                    k.Host(options.BootstrapServers);
                    k.TopicEndpoint<ScanRequestMessage>(
                        options.ScanRequestsTopic,
                        options.ScanRequestsConsumerGroup,
                        e =>
                        {
                            e.PrefetchCount = options.ScanRequestsConcurrentMessagesLimit * 2;
                            e.ConcurrentConsumerLimit = options.ScanRequestsConcurrentConsumerLimit;
                            e.ConcurrentMessageLimit = options.ScanRequestsConcurrentMessagesLimit;
                            e.ConcurrentDeliveryLimit = options.ScanRequestsConcurrentMessagesLimit;
                            e.CheckpointInterval = TimeSpan.FromSeconds(5);

                            e.ConfigureConsumer<ScanRequestConsumer>(context);
                        });
                });
            });
        });

        return services;
    }
}