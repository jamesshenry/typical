using Microsoft.Extensions.DependencyInjection;

namespace Typical.DataAccess.LiteDB;

public static class ServiceExtensions
{
    public static IServiceCollection AddTypeTypeDb(
        this IServiceCollection services,
        string connectionString
    )
    {
        services.AddSingleton(sp => new DbContext(connectionString));
        return services;
    }
}
