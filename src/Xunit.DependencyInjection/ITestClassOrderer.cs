namespace Xunit.DependencyInjection;

/// <summary>
/// A class implements this interface to participate in ordering tests for the test runner.
/// </summary>
public interface ITestClassOrderer
{
    /// <summary>
    /// Orders test cases for execution.
    /// </summary>
    /// <param name="testCases">The test cases to be ordered.</param>
    /// <returns>The test cases in the order to be run.</returns>
    IEnumerable<ITestClass> OrderTestClasses(IEnumerable<ITestClass> testCases);
}
