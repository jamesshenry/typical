using Typical.Core.Statistics;

namespace Typical.Core.Data;

public interface IStatsRepository
{
    TestResult GetTestResultAsync();
    Task SaveTestResultAsync(TestResult result);
}
