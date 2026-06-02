using System.Reflection;
using System.Text;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Typical.Core.Data;
using Typical.Core.Statistics;

namespace Typical.DataAccess.Sqlite;

public class StatsRepository(IOptions<TypicalDbOptions> options) : IStatsRepository
{
    public async Task SaveTestResultAsync(TestResult result)
    {
        await using var connection = await GetConnectionAsync();
        await using var transaction = connection.BeginTransaction();

        try
        {
            long testId = await InsertTestHeaderAsync(connection, transaction, result);

            await InsertTelemetryAsync(connection, transaction, testId, result);

            await InsertSnapshotsAsync(connection, transaction, testId, result);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<long> InsertTestHeaderAsync(
        SqliteConnection conn,
        SqliteTransaction trans,
        TestResult result
    )
    {
        const string sql = """
            INSERT INTO Tests (CreatedAt, Wpm, RawWpm, Accuracy, DurationMs, QuoteId, CustomText)
            VALUES (@CreatedAt, @Wpm, @RawWpm, @Accuracy, @DurationMs, @QuoteId, @CustomText);
            SELECT last_insert_rowid();
            """;

        await using var cmd = new SqliteCommand(sql, conn, trans);

        cmd.Parameters.AddWithValue(
            "@CreatedAt",
            new DateTimeOffset(result.PlayedAt).ToUnixTimeMilliseconds()
        );
        cmd.Parameters.AddWithValue("@Wpm", result.FinalWpm.Value);
        cmd.Parameters.AddWithValue("@RawWpm", result.RawWpm.Value);
        cmd.Parameters.AddWithValue("@Accuracy", result.FinalAccuracy.Value);
        cmd.Parameters.AddWithValue("@DurationMs", (long)result.Duration.TotalMilliseconds);

        if (result.Target.SourceId.HasValue)
        {
            cmd.Parameters.AddWithValue("@QuoteId", result.Target.SourceId.Value);
            cmd.Parameters.AddWithValue("@CustomText", DBNull.Value);
        }
        else
        {
            cmd.Parameters.AddWithValue("@QuoteId", DBNull.Value);
            cmd.Parameters.AddWithValue("@CustomText", result.Target.Text);
        }

        return (long)(await cmd.ExecuteScalarAsync() ?? 0L);
    }

    private async Task InsertTelemetryAsync(
        SqliteConnection conn,
        SqliteTransaction trans,
        long testId,
        TestResult result
    )
    {
        const string sql = """
            INSERT INTO KeystrokeTelemetry (TestId, OffsetMs, GraphemeIndex, ActualText, KeystrokeType)
            VALUES (@TestId, @OffsetMs, @Index, @Actual, @Type);
            """;

        await using var cmd = new SqliteCommand(sql, conn, trans);
        var pTestId = cmd.Parameters.Add("@TestId", SqliteType.Integer);
        var pOffset = cmd.Parameters.Add("@OffsetMs", SqliteType.Integer);
        var pIndex = cmd.Parameters.Add("@Index", SqliteType.Integer);
        var pActual = cmd.Parameters.Add("@Actual", SqliteType.Text);
        var pType = cmd.Parameters.Add("@Type", SqliteType.Integer);

        pTestId.Value = testId;

        foreach (var log in result.Telemetry)
        {
            pOffset.Value = log.Timestamp;
            pIndex.Value = log.Index;
            pActual.Value = log.Value;
            pType.Value = (int)log.Type;

            await cmd.ExecuteNonQueryAsync();
        }
    }

    private async Task InsertSnapshotsAsync(
        SqliteConnection conn,
        SqliteTransaction trans,
        long testId,
        TestResult result
    )
    {
        if (result.Snapshots.Count == 0)
            return;

        const string sql = """
            INSERT INTO TestSnapshots (TestId, OffsetMs, Wpm, Accuracy)
            VALUES (@TestId, @OffsetMs, @Wpm, @Acc);
            """;

        await using var cmd = new SqliteCommand(sql, conn, trans);
        var pTestId = cmd.Parameters.Add("@TestId", SqliteType.Integer);
        var pOffset = cmd.Parameters.Add("@OffsetMs", SqliteType.Integer);
        var pWpm = cmd.Parameters.Add("@Wpm", SqliteType.Real);
        var pAcc = cmd.Parameters.Add("@Acc", SqliteType.Real);

        pTestId.Value = testId;

        foreach (var snap in result.Snapshots)
        {
            pTestId.Value = testId;
            pOffset.Value = (long)snap.ElapsedTime.TotalMilliseconds;

            pWpm.Value = snap.WPM.Value;
            pAcc.Value = snap.Accuracy.Value;

            await cmd.ExecuteNonQueryAsync();
        }
    }

    private async Task<SqliteConnection> GetConnectionAsync()
    {
        var connection = new SqliteConnection(options.Value.GetConnectionString());
        await connection.OpenAsync();
        return connection;
    }
}
