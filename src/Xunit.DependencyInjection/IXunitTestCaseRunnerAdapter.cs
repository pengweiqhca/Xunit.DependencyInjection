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
    /// <param name="testCase">Test case.</param>
    /// <param name="context">The dependency injection context.</param>
    /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages to.</param>
    /// <param name="messageBus">The message bus to report results to.</param>
    /// <param name="constructorArguments">The arguments to pass to the constructor.</param>
    /// <param name="aggregator">The error aggregator to use for catching exception.</param>
    /// <param name="cancellationTokenSource">The cancellation token source that indicates whether cancellation has been requested.</param>
    /// <returns>Returns the summary of the test case run.</returns>
    Task<RunSummary> RunAsync(IXunitTestCase testCase,
        DependencyInjectionContext context,
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        object?[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource);
}
