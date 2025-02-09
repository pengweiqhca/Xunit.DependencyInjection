namespace Xunit.DependencyInjection.Test.Parallelization2;

public abstract class BaseTest(MaxParallelThreadsMonitor monitor)
{
    [Fact]
    public async Task TestOne()
    {
        monitor.Execute();

        await Task.Delay(1000);
    }
}

public class TestClass1(MaxParallelThreadsMonitor monitor) : BaseTest(monitor);

public class TestClass2(MaxParallelThreadsMonitor monitor) : BaseTest(monitor);
