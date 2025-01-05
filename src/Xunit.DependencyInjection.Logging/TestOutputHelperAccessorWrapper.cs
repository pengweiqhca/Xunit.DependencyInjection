namespace Xunit.DependencyInjection.Logging;

internal sealed class TestOutputHelperAccessorWrapper(ITestOutputHelperAccessor accessor)
    : MartinCostello.Logging.XUnit.ITestOutputHelperAccessor
{
    ITestOutputHelper? MartinCostello.Logging.XUnit.ITestOutputHelperAccessor.OutputHelper
    {
        get => accessor.Output;
        set => throw new NotSupportedException();
    }
}
