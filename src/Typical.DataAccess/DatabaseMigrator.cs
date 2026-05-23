using System.IO;
using DbUp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Typical.DataAccess.Sqlite;

public class DatabaseMigrator(IOptions<TypicalDbOptions> options, ILogger<DatabaseMigrator> logger)
    : IDatabaseMigrator
{
    public Task EnsureDatabaseUpdated()
    {
        try
        {
            logger.LogInformation("Opening Db");
            var connectionString = options.Value.GetConnectionString();
            var scriptsDirectory = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, options.Value.ScriptsDirectory)
            );

            logger.LogInformation("ConnectionString: {ConnectionString}", connectionString);
            logger.LogInformation(
                "Loading DbUp scripts from filesystem: {ScriptsDirectory}",
                scriptsDirectory
            );

            var upgrader = DeployChanges
                .To.SqliteDatabase(connectionString)
                .WithGeneratedScripts()
                .WithScriptsFromFileSystem(scriptsDirectory)
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
        catch
        {
            logger.LogCritical(
                "Error when migrating db at {dbPath}",
                options.Value.GetDatabasePath()
            );
            throw;
        }
    }
}
