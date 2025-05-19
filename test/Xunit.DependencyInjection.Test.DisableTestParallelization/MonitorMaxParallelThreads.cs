using System.Collections.Concurrent;

namespace Xunit.DependencyInjection.Test.Parallelization;

public class MonitorMaxParallelThreads(ITestOutputHelperAccessor output)
{
    private readonly ConcurrentBag<DateTime> _startTimes = [];

    public IEnumerable<DateTime> StartTimes => _startTimes.OrderBy(x => x);

    public void Execute()
    {
        output.Output?.WriteLine(DateTime.Now.ToString("mm:ss.fff"));

        _startTimes.Add(DateTime.Now);
    }
}
