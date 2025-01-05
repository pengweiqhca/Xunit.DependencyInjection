namespace Xunit.DependencyInjection;

public interface ITestOutputHelperAccessor
{
    ITestOutputHelper? Output { get; }
}

public class TestOutputHelperAccessor : ContextValue<ITestOutputHelper>, ITestOutputHelperAccessor
{
    public ITestOutputHelper? Output => TestContext.Current.TestOutputHelper;
}
