using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Infrastructure.BackgroundJobs;
using NetworkMapper.Infrastructure.Persistence;
using NetworkMapper.Infrastructure.Persistence.Interceptors;
using Quartz;

namespace NetworkMapper.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")!;
        services.AddSingleton<CreateScanOutboxMessagesInterceptor>();
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<CreateScanOutboxMessagesInterceptor>();
            options.UseNpgsql(connectionString)
                .AddInterceptors(interceptor);
        }); 
        
        services.AddScoped<DbContext, AppDbContext>();

        services.Scan(scan => scan
            .FromAssemblies(AssemblyReference.Assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IRepository<,>)))
            .AsImplementedInterfaces());

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddQuartz(configure =>
        {
            var jobKey = new JobKey(nameof(ProcessOutboxMessagesJob));
            configure
                .AddJob<ProcessOutboxMessagesJob>(opts => opts.WithIdentity(jobKey))
                .AddTrigger(trigger =>
                    trigger.ForJob(jobKey)
                        .WithSimpleSchedule(schedule =>
                            schedule.WithIntervalInSeconds(10)
                                .RepeatForever()));
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
        return services;
    }
}