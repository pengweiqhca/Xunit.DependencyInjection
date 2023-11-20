namespace Xunit.DependencyInjection.Test;

public class TestCaseByMethodNameOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
        where TTestCase : ITestCase =>
        testCases.OrderBy(t => t.TestMethod.Method.Name);
}
