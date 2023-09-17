namespace Xunit.DependencyInjection;

public class DependencyInjectionTestCaseRunnerWrapper : IXunitTestCaseRunnerWrapper
{
    /// <inheritdoc />
    public virtual Type TestCaseType => typeof(XunitTestCase);

    /// <inheritdoc />
    public virtual Task<RunSummary> RunAsync(IXunitTestCase testCase,
        DependencyInjectionContext context,
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        object?[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource) =>
        new DependencyInjectionTestCaseRunner(context, testCase, testCase.DisplayName,
            testCase.SkipReason, constructorArguments, testCase.TestMethodArguments,
            messageBus, aggregator, cancellationTokenSource).RunAsync();
}
