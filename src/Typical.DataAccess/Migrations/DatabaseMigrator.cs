using DbUp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Typical.DataAccess.Sqlite;

public class DatabaseMigrator(IOptions<TypicalDbOptions> options, ILogger<DatabaseMigrator> logger)
    : IDatabaseMigrator
{
    public Task EnsureDatabaseUpdated()
    {
        logger.LogInformation("Opening Db");
        var connectionString = options.Value.GetConnectionString();

        logger.LogInformation("ConnectionString: {ConnectionString}", connectionString);

        var upgrader = DeployChanges
            .To.SqliteDatabase(connectionString)
            .WithGeneratedScripts()
            .LogTo(logger)
            .LogScriptOutput()
            .Build();

        logger.LogInformation("Upgrader built");
        logger.LogInformation("Performing upgrade");

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            logger.LogError(result.Error, "Database upgrade failed");
            throw result.Error;
        }

        logger.LogInformation("Done");
        return Task.CompletedTask;
    }
}
