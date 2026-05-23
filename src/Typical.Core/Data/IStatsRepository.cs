using Typical.Core.Statistics;

namespace Typical.Core.Data;

public interface IStatsRepository
{
    Task SaveGameResultAsync(TestResult result);
}
