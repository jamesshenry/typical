using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Typical.Core.Statistics;

namespace Typical.DataAccess.Sqlite;

public class StatsRepository(IOptions<TypicalDbOptions> options) : IStatsRepository
{
    public async Task SaveGameResultAsync(TestResult result)
    {
        await using var connection = await GetConnectionAsync();
        await using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Insert the Test Header
            long testId = await InsertTestHeaderAsync(connection, transaction, result);

            // 2. Insert Keystroke Telemetry (Bulk)
            await InsertTelemetryAsync(connection, transaction, testId, result);

            // 3. Insert Snapshots (Bulk)
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
            INSERT INTO Tests (CreatedAt, Wpm, RawWpm, Accuracy, DurationMs, TargetText, Source)
            VALUES (@CreatedAt, @Wpm, @RawWpm, @Accuracy, @DurationMs, @TargetText, @Source);
            SELECT last_insert_rowid();
            """;

        await using var cmd = new SqliteCommand(sql, conn, trans);

        // Convert DateTime to Unix Milliseconds (Standard for SQLite INTEGER)
        cmd.Parameters.AddWithValue(
            "@CreatedAt",
            new DateTimeOffset(result.PlayedAt).ToUnixTimeMilliseconds()
        );
        cmd.Parameters.AddWithValue("@Wpm", result.FinalWpm.Value);
        cmd.Parameters.AddWithValue("@RawWpm", result.RawWpm.Value);
        cmd.Parameters.AddWithValue("@Accuracy", result.FinalAccuracy.Value);
        cmd.Parameters.AddWithValue("@DurationMs", (long)result.Duration.TotalMilliseconds);
        cmd.Parameters.AddWithValue("@TargetText", result.Target);
        cmd.Parameters.AddWithValue("@Source", result.Target.Source ?? (object)DBNull.Value);

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

        // Note: result.Telemetry is your List<KeystrokeLog>
        foreach (var log in result.Telemetry)
        {
            pOffset.Value = log.Timestamp; // Relative offset from game start
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
