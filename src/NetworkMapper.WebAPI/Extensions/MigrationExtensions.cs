using Microsoft.EntityFrameworkCore;
using NetworkMapper.Infrastructure.Persistence;

namespace NetworkMapper.WebAPI.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<DbContext>();
        try
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger("DatabaseMigrations"); 
            
            logger.LogError(ex, "An error occurred while applying the database migrations.");
            throw;
        }
    }
}