namespace Xunit.DependencyInjection.Test;

public interface IDependency
{
    int Value { get; set; }

    int TestWriteLine(int count);
}

internal class DependencyClass(ITestOutputHelperAccessor testOutputHelperAccessor) : IDependency, IAsyncDisposable
{
    public int Value { get; set; }

    public int TestWriteLine(int count)
    {
        var output = testOutputHelperAccessor.Output;
        if (output != null)
            for (var index = 0; index < count; index++)
            {
                output.WriteLine($"{DateTime.Now:ss.fff} test {index}");
                Thread.Sleep(1);
            }

        return 1;
    }

    public ValueTask DisposeAsync() => default;
}