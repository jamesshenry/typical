using System.Data;
using System.Data.Common;
using System.Text.Json;
using Dapper;
using DbUp;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Typical.Core.Data;
using Typical.DataAccess.Sqlite;

[assembly: DbUpGenerateScripts]
[module: DapperAot(true)]

namespace Typical.DataAccess.Sqlite;

public class TextRepository(IOptions<TypicalDbOptions> options) : ITextRepository
{
    private async Task<DbConnection> GetOpenConnectionAsync()
    {
        var connection = new SqliteConnection(options.Value.GetConnectionString());
        await connection.OpenAsync();
        return connection;
    }

    public async Task<Quote> GetQuoteAsync(int id)
    {
        await using var connection = await GetOpenConnectionAsync();

        string sql =
            @"
        SELECT Id, Text, Author, Tags, WordCount, CharCount 
        FROM Quotes 
        ORDER BY Id ASC LIMIT 1;";

        var dto = await connection.QueryFirstAsync<QuoteDto>(sql);

        return dto.ToQuote();
    }

    public async Task<Quote> GetRandomQuoteAsync()
    {
        await using var connection = await GetOpenConnectionAsync();
        await connection.OpenAsync();

        string sql =
            "SELECT Id, Text, Author, Tags, WordCount, CharCount FROM Quotes ORDER BY RANDOM() LIMIT 1";

        var quote1 = await connection.QueryFirstAsync<QuoteDto>(sql);

        return quote1.ToQuote();
    }

    public Task<bool> HasAnyAsync()
    {
        throw new NotImplementedException();
    }
}
