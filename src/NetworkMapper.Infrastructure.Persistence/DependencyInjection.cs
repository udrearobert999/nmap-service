using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetworkMapper.Domain.Abstractions;
using NetworkMapper.Infrastructure.Persistence.Interceptors;

namespace NetworkMapper.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
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
        
        return services;
    }
}