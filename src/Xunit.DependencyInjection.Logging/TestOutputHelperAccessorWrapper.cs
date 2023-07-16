using Xunit.Abstractions;

namespace Xunit.DependencyInjection.Logging;

internal sealed class TestOutputHelperAccessorWrapper
    : MartinCostello.Logging.XUnit.ITestOutputHelperAccessor
{
    private readonly ITestOutputHelperAccessor _accessor;

    public TestOutputHelperAccessorWrapper(ITestOutputHelperAccessor accessor) => _accessor = accessor;

    ITestOutputHelper? MartinCostello.Logging.XUnit.ITestOutputHelperAccessor.OutputHelper
    {
        get => _accessor.Output;
        set => _accessor.Output = value;
    }
}
