using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Typical.DataAccess;
using Typical.DataAccess.Sqlite;

namespace Typical.Tests.Data;

public class TextRepositoryTests
{
    private static async Task<(SqliteConnection, TextRepository)> CreateInMemoryRepoAsync()
    {
        // Use a unique in-memory database name per test run to avoid schema conflicts
        var dbName = $"memdb_{Guid.NewGuid()}";
        var connectionString = $"Data Source=file:{dbName}?mode=memory&cache=shared";
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        var options = new TypicalDbOptions
        {
            DataDirectory = "",
            DatabaseFileName = $"file:{dbName}?mode=memory&cache=shared",
        };
        var optionsWrapper = new Microsoft.Extensions.Options.OptionsWrapper<TypicalDbOptions>(
            options
        );
        var migrator = new DatabaseMigrator(optionsWrapper, NullLogger<DatabaseMigrator>.Instance);
        await migrator.EnsureDatabaseUpdated();
        var repo = new TextRepository(optionsWrapper);
        return (connection, repo);
    }

    [Test]
    public async Task GetRandomQuoteAsync_ReturnsSeededQuote()
    {
        var (conn, repo) = await CreateInMemoryRepoAsync();
        var quote = await repo.GetRandomQuoteAsync();
        await Assert.That(quote).IsNotNull();
        await Assert.That(quote.Text).IsNotEmpty();
        await Assert.That(quote.Author).IsNotEmpty();
        await conn.DisposeAsync();
    }

    [Test]
    public async Task GetQuoteAsync_MapsDBNullAuthor_ToUnknown()
    {
        var (conn, repo) = await CreateInMemoryRepoAsync();
        // Insert a quote with NULL author
        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Quotes (Text, Author) VALUES (@text, NULL);";
        cmd.Parameters.AddWithValue("@text", "Anonymous wisdom");
        await cmd.ExecuteNonQueryAsync();
        // Get the last inserted row
        cmd.CommandText = "SELECT last_insert_rowid();";
        var id = (long)await cmd.ExecuteScalarAsync();
        var quote = await repo.GetQuoteByIdAsync((int)id);
        await Assert.That(quote.Author).IsEqualTo("Unknown");
        await conn.DisposeAsync();
    }
}
