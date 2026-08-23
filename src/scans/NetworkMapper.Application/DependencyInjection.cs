using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NetworkMapper.Application.Validation;

namespace NetworkMapper.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var currentAssembly = AssemblyReference.Assembly;

        services.Scan(scan => scan
            .FromAssemblies(currentAssembly)
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")), false)
            .AsImplementedInterfaces()
            .WithTransientLifetime());
        
        services.AddValidatorsFromAssembly(currentAssembly, ServiceLifetime.Transient);

        services.AddTransient<IValidationOrchestrator, ValidationOrchestrator>();

        return services;
    }
}