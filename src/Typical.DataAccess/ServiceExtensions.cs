using Microsoft.Extensions.DependencyInjection;
using Typical.DataAccess.Sqlite;

namespace Typical.DataAccess;

public static class ServiceExtensions
{
    public static IServiceCollection AddTypicalDb(
        this IServiceCollection services,
        string connectionString
    )
    {
        services.AddSingleton<IDatabaseMigrator, DatabaseMigrator>();
        return services;
    }
}
