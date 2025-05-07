using System.Collections.Concurrent;

namespace Xunit.DependencyInjection.Test.DisableTestParallelization;

public class MaxParallelThreadsMonitor
{
    private readonly ConcurrentBag<DateTime> _startTimes = [];

    public IEnumerable<DateTime> StartTimes => _startTimes.OrderBy(x => x);

    public void Execute() => _startTimes.Add(DateTime.Now);
}
