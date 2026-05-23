using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Typical.Core.Data;
using Typical.Core.Statistics;

namespace Typical.DataAccess.Sqlite;

public class StatsRepository(IOptions<TypicalDbOptions> options) : IStatsRepository
{
    public async Task SaveGameResultAsync(TestResult result)
    {
        await using var connection = await GetConnectionAsync();
        await using var command = connection.CreateCommand();
    }

    private async Task<SqliteConnection> GetConnectionAsync()
    {
        var connection = new SqliteConnection(options.Value.GetConnectionString());
        await connection.OpenAsync();
        return connection;
    }
}
