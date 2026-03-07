using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkMapper.Application.Worker;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var currentAssembly = AssemblyReference.Assembly;

        services.Scan(scan => scan
            .FromAssemblies(currentAssembly)
            .AddClasses(classes => classes.Where(type =>
                type.Name.EndsWith("Service") || type.Name.EndsWith("Runner") || type.Name.EndsWith("Parser")), false)
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        return services;
    }
}