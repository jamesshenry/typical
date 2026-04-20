using System.Text.Json;
using DbUp;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Typical.Core.Data;

[assembly: DbUpGenerateScripts]

namespace Typical.DataAccess.Sqlite;

public class TextRepository(IOptions<TypicalDbOptions> options) : ITextRepository
{
    private SqliteConnection CreateConnection() => new(options.Value.GetConnectionString());

    public Task AddQuotesAsync(IEnumerable<Quote> quotes)
    {
        throw new NotImplementedException();
    }

    public async Task<Quote> GetQuoteAsync(int id)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText =
            @"
        SELECT Id, Text, Author, Tags, WordCount, CharCount 
        FROM Quotes 
        WHERE Id > @id 
        ORDER BY Id ASC LIMIT 1;";
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapReaderToQuote(reader);
        }

        command.CommandText =
            @"
        SELECT Id, Text, Author, Tags, WordCount, CharCount 
        FROM Quotes 
        ORDER BY Id ASC LIMIT 1;";

        await using var wrapReader = await command.ExecuteReaderAsync();
        if (await wrapReader.ReadAsync())
        {
            return MapReaderToQuote(wrapReader);
        }

        throw new InvalidOperationException(
            "No quotes found in the database. Ensure the migration and seeding scripts ran successfully."
        );
    }

    public async Task<Quote> GetRandomQuoteAsync()
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT Id, Text, Author, Tags, WordCount, CharCount FROM Quotes ORDER BY RANDOM() LIMIT 1";

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapReaderToQuote(reader);
        }

        throw new InvalidOperationException(
            "No quotes found in the database. Ensure the migration and seeding scripts ran successfully."
        );
    }

    public Task<bool> HasAnyAsync()
    {
        throw new NotImplementedException();
    }

    private static Quote MapReaderToQuote(SqliteDataReader reader)
    {
        var tagsJson = reader.IsDBNull(3) ? null : reader.GetString(3);

        return new Quote
        {
            Id = reader.GetInt32(0),
            Text = reader.GetString(1),
            Author = reader.IsDBNull(2) ? "Unknown" : reader.GetString(2),
            // AOT-Safe deserialization
            Tags =
                tagsJson != null
                    ? JsonSerializer.Deserialize(tagsJson, SeedContext.Default.ListString) ?? []
                    : [],
            WordCount = reader.GetInt32(4),
            CharCount = reader.GetInt32(5),
        };
    }
}

public interface IDatabaseMigrator
{
    Task EnsureDatabaseUpdated();
}
