using Shouldly;

namespace Xunit.DependencyInjection.Test.Parallelization;

public class MaxParallelThreadsMonitorTest(MaxParallelThreadsMonitor monitor, ITestOutputHelper output)
{
    [Fact]
    public void MonitorTest()
    {
        var startTimes = monitor.StartTimes.ToArray();

        Assert.Equal(6, startTimes.Length);

        var diff = GetDiff(startTimes);

        for (var i = 0; i < diff.Length; i++)
        {
#if DisableTestParallelization
            diff[i].ShouldBeInRange(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1100), "i = " + i);
#else
            if (i % 3 == 0)
                diff[i].ShouldBeLessThan(TimeSpan.FromMilliseconds(100), "i = " + i);
            else
                diff[i].ShouldBeInRange(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1100), "i = " + i);
#endif
        }
    }

    private TimeSpan[] GetDiff(DateTime[] startTimes)
    {
        foreach (var startTime in startTimes)
            output.WriteLine(startTime.ToString("mm:ss.fff"));

        var diff = new TimeSpan[startTimes.Length - 1];

        for (var i = 0; i < diff.Length; i++)
        {
            startTimes[i].ShouldBeLessThanOrEqualTo(startTimes[i + 1], "i = " + i);

            diff[i] = startTimes[i + 1] - startTimes[i];
        }

        return diff;
    }
}
