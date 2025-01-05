namespace Xunit.DependencyInjection;

public class DependencyInjectionTestCaseRunnerWrapper : IXunitTestCaseRunnerWrapper
{
    /// <inheritdoc />
    public virtual Type TestCaseType => typeof(XunitTestCase);

    public ValueTask<RunSummary> RunAsync(DependencyInjectionContext context, IXunitTestCase testCase,
        IReadOnlyCollection<IXunitTest> tests,
        IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource,
        string displayName, string? skipReason, ExplicitOption explicitOption, object?[] constructorArguments) =>
        new DependencyInjectionTestCaseRunner(context).Run(
            testCase,
            tests,
            messageBus,
            aggregator,
            cancellationTokenSource,
            displayName,
            skipReason,
            explicitOption,
            constructorArguments);
}
