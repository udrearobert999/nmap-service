using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetworkMapper.Contracts.Scans.Messages;
using NetworkMapper.Infrastructure.BackgroundJobs;
using NetworkMapper.Infrastructure.Options;
using Quartz;

namespace NetworkMapper.Infrastructure;

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