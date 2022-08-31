using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;

#pragma warning disable CS8629
#pragma warning disable CS8631

namespace Ordering.API.Extensions;

public static class HostExtension
{
    public static IHost MigrateDatabase<TContext>(this IHost host, Action<TContext, IServiceProvider> seeder) where TContext : DbContext
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<TContext>>();
        var context = services.GetService<TContext>();

        try
        {
            logger.LogInformation("Migrating database associated with {DbContextName}",  typeof(TContext).Name);

            var retry = Policy.Handle<SqlException>()
                .WaitAndRetry(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempts => TimeSpan.FromSeconds(Math.Pow(2, retryAttempts)),
                    onRetry: (exception, retryCount, context) =>
                    {
                        logger.LogError($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey} due to: {exception}");
                    });

            retry.Execute(() => InvokeSeeder(seeder!, context, services));
            
            
            
            logger.LogInformation("Migrated database associated with {DbContextName}",  typeof(TContext).Name);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
        }

        return host;
    }

    private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext context, IServiceProvider services) where TContext : DbContext
    {
        context.Database.Migrate();
        seeder(context, services);
    }
}