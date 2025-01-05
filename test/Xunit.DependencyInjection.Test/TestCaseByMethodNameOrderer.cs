using Xunit.v3;

namespace Xunit.DependencyInjection.Test;

public class TestCaseByMethodNameOrderer : ITestCaseOrderer
{
    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases) where TTestCase : notnull, ITestCase =>
        testCases.OrderBy(t => t.TestMethod?.MethodName).ToArray();
}
