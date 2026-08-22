using Xunit.v3;

namespace Xunit.DependencyInjection.Test;

public class TestCaseByMethodNameOrderer : ITestMethodOrderer
{
    public IReadOnlyCollection<TTestMethod?> OrderTestMethods<TTestMethod>(IReadOnlyCollection<TTestMethod?> testMethods) where TTestMethod : ITestMethod =>
        [.. testMethods.OrderBy(t => t?.MethodName)];
}
