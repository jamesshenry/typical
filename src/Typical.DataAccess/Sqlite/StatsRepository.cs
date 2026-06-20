using System.ComponentModel.Design;
using System.Reflection;
using System.Text;

using Dapper;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

using Typical.Core.Data;
using Typical.Core.Statistics;
using Typical.Core.Text;

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

            await InsertSnapshotsAsync(connection, transaction, testId, result.Snapshots);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<TestResult> GetTestResultAsync(int? id = null)
    {
        await using var connection = await GetConnectionAsync();

        if (id is null)
        {
            int maxId = await connection.ExecuteScalarAsync<int>(@"SELECT MAX(id) FROM Tests;");
            id = Random.Shared.Next(1, maxId + 1);
        }

        const string testSql =
            @"
        SELECT
            t.Wpm,
            t.CreatedAt,
            t.Accuracy,
            t.DurationMs,
            q.Id as QuoteId,
            q.Text as QuoteText,
            q.Author as QuoteAuthor,
            q.CharCount,
            q.WordCount
        FROM Tests t
        LEFT JOIN Quotes q ON t.QuoteId = q.Id
        WHERE t.Id = @testId;
        ";

        var testRow = await connection.QueryFirstOrDefaultAsync<TestRow>(
            testSql,
            new { testId = id }
        );

        const string snapshotsSql =
            @"
        SELECT * FROM TestSnapshots WHERE TestId = @testId ORDER BY OffsetMs ASC;
        ";

        var snapshots = (
            await connection.QueryAsync<TestSnapshot>(snapshotsSql, new { testId = id })
        ).ToList();

        TextSample sample;
        if (testRow?.QuoteId is not null)
        {
            sample = new TextSample
            {
                SourceId = testRow.QuoteId,
                Text = testRow.QuoteText!,
                Source = testRow.QuoteAuthor ?? "Unknown",
                WordCount = testRow.WordCount,
                CharCount = testRow.CharCount,
            };
        }
        else
        {
            sample = TextSample.Empty;
        }

        return new TestResult(
            PlayedAt: DateTimeOffset.FromUnixTimeSeconds(testRow!.CreatedAt).DateTime,
            FinalWpm: Wpm.From(testRow.Wpm),
            FinalAccuracy: Accuracy.From(testRow.Accuracy),
            Duration: TimeSpan.FromMilliseconds(testRow.DurationMs),
            Target: sample,
            Telemetry: [],
            Snapshots: snapshots,
            RawWpm: Wpm.From(testRow.Wpm)
        );
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
        IEnumerable<TestSnapshot> snapshots
    )
    {
        if (snapshots.Count() == 0)
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

        foreach (var snap in snapshots)
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

internal class TestRow
{
    public float Wpm { get; set; }
    public long CreatedAt { get; set; }
    public float Accuracy { get; set; }
    public double DurationMs { get; set; }
    public int? QuoteId { get; set; }
    public string? QuoteText { get; set; }
    public string? QuoteAuthor { get; set; }
    public int CharCount { get; set; }
    public int WordCount { get; set; }
}
