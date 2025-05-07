namespace Xunit.DependencyInjection.Test.Parallelization;

public class MaxParallelThreadsMonitorTest(MaxParallelThreadsMonitor monitor)
{
    [Fact]
    public void MonitorTest()
    {
        var startTimes = monitor.StartTimes.ToArray();

        Assert.Equal(2, startTimes.Length);

        Assert.True(startTimes[0] < startTimes[1]);
        Assert.InRange(startTimes[1] - startTimes[0], TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1100));
    }
}
