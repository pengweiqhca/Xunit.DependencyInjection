namespace Xunit.DependencyInjection;

public interface ITestOutputHelperAccessor
{
    ITestOutputHelper? Output { get; set; }
}

public class TestOutputHelperAccessor : ContextValue<ITestOutputHelper>, ITestOutputHelperAccessor, IDisposable
{
    public ITestOutputHelper? Output
    {
        get => Value;
        set => Value = value;
    }

    public void Dispose()
    {
        Value = null;

        GC.SuppressFinalize(this);
    }
}
