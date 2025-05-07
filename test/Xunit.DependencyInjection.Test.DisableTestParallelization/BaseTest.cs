namespace Xunit.DependencyInjection.Test.Parallelization;

public abstract class BaseTest(MaxParallelThreadsMonitor monitor)
{
    [Fact]
    public async Task Test1()
    {
        monitor.Execute();

        await Task.Delay(1000);
    }

    [Fact]
    public async Task Test2()
    {
        monitor.Execute();

        await Task.Delay(1000);
    }

    [Fact]
    public async Task Test3()
    {
        monitor.Execute();

        await Task.Delay(1000);
    }
}

public class TestClass1(MaxParallelThreadsMonitor monitor) : BaseTest(monitor);

public class TestClass2(MaxParallelThreadsMonitor monitor) : BaseTest(monitor);
