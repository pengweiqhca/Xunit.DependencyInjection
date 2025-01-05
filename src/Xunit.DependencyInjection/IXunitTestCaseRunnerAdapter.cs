namespace Xunit.DependencyInjection;

public interface IXunitTestCaseRunnerWrapper
{
    /// <summary>
    /// Support type.
    /// </summary>
    Type TestCaseType { get; }

    /// <summary>
    /// Executes the test case, returning 0 or more result messages through the message sink.
    /// </summary>
    /// <param name="context">The dependency injection context.</param>
    /// <param name="testCase">The test case that this invocation belongs to.</param>
    /// <param name="tests">The tests for the test case.</param>
    /// <param name="messageBus">The message bus to report run status to.</param>
    /// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
    /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
    /// <param name="displayName">The display name of the test case.</param>
    /// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
    /// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
    /// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
    /// <returns>Returns the summary of the test case run.</returns>
    ValueTask<RunSummary> RunAsync(DependencyInjectionContext context,
        IXunitTestCase testCase,
        IReadOnlyCollection<IXunitTest> tests,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        string displayName,
        string? skipReason,
        ExplicitOption explicitOption,
        object?[] constructorArguments);
}
