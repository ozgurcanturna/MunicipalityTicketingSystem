using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharedKernel.Infrastructure.MultiTenancy;
using SharedKernel.Infrastructure.Persistence;

namespace SharedKernel.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSharedInfrastructure<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder, string>? dbOptions = null)
        where TDbContext : AppDbContext
    {
        services.TryAddScoped<ITenantProvider, NullTenantProvider>();
        services.TryAddSingleton<TenantConnectionStringResolver>();

        services.AddDbContext<TDbContext>((serviceProvider, options) =>
        {
            var tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            var resolver = serviceProvider.GetRequiredService<TenantConnectionStringResolver>();
            var connectionString = resolver.Resolve(tenantProvider.TenantId);
            PostgresDatabaseInitializer.EnsureDatabaseExists(connectionString);

            if (dbOptions is not null)
            {
                dbOptions(options, connectionString);
                return;
            }

            options.UseNpgsql(connectionString);
        });

        ConfigureRedisCache(services, configuration);

        return services;
    }

    private static void ConfigureRedisCache(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "MunicipalityTicketing";
        });
    }
}