using System.Diagnostics;
using Typical.Core.Data;
using Typical.Core.Statistics;

namespace Typical.DataAccess.Sqlite;

public class SimpleStatsRepository : IStatsRepository
{
    public Task SaveGameResultAsync(TestResult result)
    {
        Debug.WriteLine("SimpleStatsRepository");

        Debug.WriteLine(result.ToString());

        return Task.CompletedTask;
    }
}
