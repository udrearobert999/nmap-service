using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetworkMapper.Domain.Abstractions;

namespace NetworkMapper.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        services.AddDbContext<DbContext, AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
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