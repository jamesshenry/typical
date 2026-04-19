using DbUp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Typical.DataAccess.Sqlite;

public class DatabaseMigrator(IConfiguration configuration, ILogger<DatabaseMigrator> logger)
    : IDatabaseMigrator
{
    public Task EnsureDatabaseUpdated()
    {
        logger.LogInformation("Opening Db");
        var connectionString = configuration.GetConnectionString("Default");

        logger.LogInformation("ConnectionString: {ConnectionString}", connectionString);

        var upgrader = DeployChanges
            .To.SqliteDatabase(connectionString)
            .WithGeneratedScripts()
            .LogTo(logger)
            .LogToConsole()
            .Build();

        logger.LogInformation("Upgrader built");
        logger.LogInformation("Performing upgrade");

        var results = upgrader.PerformUpgrade();

        logger.LogInformation("Done");
        return Task.CompletedTask;
    }
}
