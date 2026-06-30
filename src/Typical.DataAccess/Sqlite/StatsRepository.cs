using System.ComponentModel.Design;
using System.Reflection;
using System.Text;

using Dapper;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

using Typical.Core.Data;
using Typical.Core.Exceptions;
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
            int? maxId = await connection.ExecuteScalarAsync<int?>(@"SELECT MAX(id) FROM Tests;");

            if (maxId is null)
            {
                throw new TypicalException();
            }
            id = Random.Shared.Next(1, maxId.Value + 1);
        }
        var testIdParam = new { testId = id };

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

        var testRow = await connection.QueryFirstOrDefaultAsync<TestDto>(
            testSql,
            testIdParam
        );

        const string snapshotsSql =
            @"
        SELECT * FROM TestSnapshots WHERE TestId = @testId ORDER BY OffsetMs ASC;
        ";

        var snapshotDtos = await connection.QueryAsync<TestSnapshotDto>(snapshotsSql, testIdParam);
        var snapshots = snapshotDtos.Select(dto => dto.ToDomain()).ToList();
        const string telemetrySql = @"
SELECT * FROM KeystrokeTelemetry kt
WHERE kt.TestId = @testId;
";

        var telemetryDto = await connection.QueryAsync<KeystrokeTelemetryDto>(telemetrySql, testIdParam);
        var telemetry = telemetryDto.Select(row => row.ToDomain()).ToList();

        TextSample sample = testRow?.QuoteId is null
            ? TextSample.Empty
            : new TextSample
            {
                SourceId = testRow.QuoteId,
                Text = testRow.QuoteText!,
                Source = testRow.QuoteAuthor ?? "Unknown",
                WordCount = testRow.WordCount,
                CharCount = testRow.CharCount,
            };
        return new TestResult(
            PlayedAt: DateTimeOffset.FromUnixTimeMilliseconds(testRow!.CreatedAt).DateTime,
            FinalWpm: Wpm.From(testRow.Wpm),
            FinalAccuracy: Accuracy.From(testRow.Accuracy),
            Duration: TimeSpan.FromMilliseconds(testRow.DurationMs),
            Target: sample,
            Telemetry: telemetry,
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

        // Create an anonymous object for the parameters.
        // Dapper.AOT unpacks this at compile time into raw ADO.NET assignments.
        var args = new
        {
            CreatedAt = new DateTimeOffset(result.PlayedAt).ToUnixTimeMilliseconds(),
            Wpm = result.FinalWpm.Value,         // Unwrap Vogen
            RawWpm = result.RawWpm.Value,        // Unwrap Vogen
            Accuracy = result.FinalAccuracy.Value, // Unwrap Vogen
            DurationMs = (long)result.Duration.TotalMilliseconds,
            QuoteId = result.Target.SourceId,
            CustomText = result.Target.SourceId.HasValue ? null : result.Target.Text
        };

        // Use ExecuteScalarAsync to grab the newly inserted ID
        return await conn.ExecuteScalarAsync<long>(sql, args, trans);
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

        var args = result.Telemetry.Select(log => new TelemetryInsertArgs
        {
            TestId = testId,
            OffsetMs = log.OffsetMs,
            GraphemeIndex = log.Index,
            ActualText = log.Value,
            KeystrokeType = (int)log.Type // Cast enum to int
        });

        await conn.ExecuteAsync(sql, args, trans);
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

        // Project your domain models to the flattened primitive args
        var args = snapshots.Select(snap => new SnapshotInsertArgs
        {
            TestId = testId,
            OffsetMs = (long)snap.ElapsedTime.TotalMilliseconds,
            Wpm = snap.WPM.Value,           // Unwrap
            Accuracy = snap.Accuracy.Value  // Unwrap
        });

        // Pass the IEnumerable directly! Dapper.AOT handles the iteration internally.
        await conn.ExecuteAsync(sql, args, trans);
    }

    private async Task<SqliteConnection> GetConnectionAsync()
    {
        var connection = new SqliteConnection(options.Value.GetConnectionString());
        await connection.OpenAsync();
        return connection;
    }
}


[Serializable]
public class NoTestsExistException : TypicalException
{
    public NoTestsExistException()
    {
    }

    public NoTestsExistException(string? message) : base(message)
    {
    }

    public NoTestsExistException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

internal class TestDto
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

[DapperAot]
internal class TestSnapshotDto
{
    public long TestId { get; set; }
    public long OffsetMs { get; set; }
    public double Wpm { get; set; }
    [DbValue(Name = "Acc")]
    public double Accuracy { get; set; }

    internal TestSnapshot ToDomain()
    {
        return new TestSnapshot(
            Core.Statistics.Wpm.From(Wpm),
            Core.Statistics.Accuracy.From(Accuracy),
            new TestMetrics(0, 0, 0), // Metrics aren't stored in the snapshot table
            TimeSpan.FromMilliseconds(OffsetMs)
        );
    }
}

[DapperAot]
internal class KeystrokeTelemetryDto
{
    public long TestId { get; set; }
    public long OffsetMs { get; set; }
    public int GraphemeIndex { get; set; }
    public string? ActualText { get; set; }
    public int KeystrokeType { get; set; }

    internal KeystrokeLog ToDomain()
    {
        return new KeystrokeLog(
            Value: ActualText ?? string.Empty,
            Type: (KeystrokeType)KeystrokeType, // Cast the raw int back to the enum

            // Note: In InsertTelemetryAsync you save log.Timestamp into the @OffsetMs parameter.
            // We map it back to both here so the domain model stays fully populated.
            Timestamp: OffsetMs,
            OffsetMs: OffsetMs,

            Index: GraphemeIndex
        );
    }
}
[DapperAot]
internal struct SnapshotInsertArgs
{
    public long TestId { get; set; }
    public long OffsetMs { get; set; }
    public double Wpm { get; set; }

    [DbValue(Name = "Acc")] // Maps to the @Acc parameter in your SQL
    public double Accuracy { get; set; }
}
[DapperAot]
internal struct TelemetryInsertArgs
{
    public long TestId { get; set; }
    public long OffsetMs { get; set; }

    [DbValue(Name = "Index")]
    public int GraphemeIndex { get; set; }

    [DbValue(Name = "Actual")]
    public string ActualText { get; set; }

    [DbValue(Name = "Type")]
    public int KeystrokeType { get; set; }
}
