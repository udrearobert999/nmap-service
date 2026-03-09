using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetworkMapper.Application.Worker.Options;

namespace NetworkMapper.Application.Worker;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<NmapOptions>()
            .Bind(configuration.GetSection(NmapOptions.SectionName));
        
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