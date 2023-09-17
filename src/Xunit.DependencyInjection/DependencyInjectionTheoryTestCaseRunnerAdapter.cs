namespace Xunit.DependencyInjection;

public class DependencyInjectionTheoryTestCaseRunnerWrapper : IXunitTestCaseRunnerWrapper
{
    /// <inheritdoc />
    public virtual Type TestCaseType => typeof(XunitTheoryTestCase);

    /// <inheritdoc />
    public virtual Task<RunSummary> RunAsync(IXunitTestCase testCase,
        DependencyInjectionContext context,
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        object?[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource) =>
        new DependencyInjectionTheoryTestCaseRunner(context, testCase, testCase.DisplayName,
            testCase.SkipReason, constructorArguments, diagnosticMessageSink, messageBus,
            aggregator, cancellationTokenSource).RunAsync();
}
