using Confluent.Kafka;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vantage.Contracts.Scans.Messages;
using Vantage.Infrastructure.BackgroundJobs;
using Vantage.Infrastructure.Options;
using Vantage.Infrastructure.Outbox;
using Quartz;

namespace Vantage.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection(KafkaOptions.SectionName));

        services.AddMassTransit(x =>
        {
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            x.AddRider(rider =>
            {
                using var serviceProvider = services.BuildServiceProvider();
                var kafkaOptions = serviceProvider.GetRequiredService<IOptions<KafkaOptions>>().Value;

                rider.AddProducer<Guid, ScanRequestMessage>(kafkaOptions.ScanRequestsTopic);
                rider.UsingKafka((context, k) =>
                {
                    k.Host(kafkaOptions.BootstrapServers);
                });
            });
        });

        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            var kafkaOptions = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
            return new ProducerBuilder<string, string>(
                new ProducerConfig { BootstrapServers = kafkaOptions.BootstrapServers }).Build();
        });

        services.AddScoped<IOutboxMessagePublisher, ScanRequestMessagePublisher>();
        services.AddScoped<IOutboxMessagePublisher, ScanRiskAssessmentRequestedMessagePublisher>();

        services.AddQuartz(configure =>
        {
            var jobKey = new JobKey(nameof(ProcessOutboxMessagesJob));
            configure
                .AddJob<ProcessOutboxMessagesJob>(opts => opts.WithIdentity(jobKey))
                .AddTrigger(trigger =>
                    trigger.ForJob(jobKey)
                        .WithSimpleSchedule(schedule =>
                            schedule.WithIntervalInSeconds(2)
                                .RepeatForever()));
        });

        services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });
        return services;
    }
}
