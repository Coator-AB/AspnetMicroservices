using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
#pragma warning disable CS8629
#pragma warning disable CS8631

namespace Ordering.API.Extensions;

public static class HostExtension
{
    public static IHost MigrateDatabase<TContext>(this IHost host, Action<TContext, IServiceProvider> seeder,
        int? retry = 0) where TContext : DbContext
    {
        var retryForAvailability = retry.Value;

        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<TContext>>();
        var context = services.GetService<TContext>();

        try
        {
            logger.LogInformation("Migrating database associated with {DbContextName}",  typeof(TContext).Name);

            InvokeSeeder(seeder!, context, services);
            
            logger.LogInformation("Migrated database associated with {DbContextName}",  typeof(TContext).Name);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);

            if (retryForAvailability < 50)
            {
                retryForAvailability++;
                System.Threading.Thread.Sleep(2000);
                MigrateDatabase<TContext>(host, seeder, retryForAvailability);
            }
        }

        return host;
    }

    private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext context, IServiceProvider services) where TContext : DbContext
    {
        context.Database.Migrate();
        seeder(context, services);
    }
}