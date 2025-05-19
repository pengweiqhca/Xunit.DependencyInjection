namespace Xunit.DependencyInjection.Test.Parallelization;

public abstract class BaseTest(MonitorMaxParallelThreads monitor)
{
    [Fact]
    public async Task Test1()
    {
        monitor.Execute();

        await Task.Delay(1000, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Test2()
    {
        monitor.Execute();

        await Task.Delay(1000, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Test3()
    {
        monitor.Execute();

        await Task.Delay(1000, TestContext.Current.CancellationToken);
    }
}

public class TestClass1(MonitorMaxParallelThreads monitor) : BaseTest(monitor);

public class TestClass2(MonitorMaxParallelThreads monitor) : BaseTest(monitor);
