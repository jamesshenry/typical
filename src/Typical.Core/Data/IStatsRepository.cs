using Typical.Core.Statistics;

namespace Typical.Core.Data;

public interface IStatsRepository
{
    Task<TestResult> GetTestResultAsync(int? id = null);
    Task SaveTestResultAsync(TestResult result);
}
