namespace Xunit.DependencyInjection;

public class DependencyInjectionTestCaseRunnerWrapper : IXunitTestCaseRunnerWrapper
{
    /// <inheritdoc />
    public virtual Type TestCaseType => typeof(XunitTestCase);

    public virtual ValueTask<RunSummary> RunAsync(DependencyInjectionContext context, IXunitTestCase testCase,
        IReadOnlyCollection<IXunitTest> tests,
        IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource,
        string displayName, string? skipReason, ExplicitOption explicitOption, object?[] constructorArguments,
        ParallelMode parallelMode, ExecutionScheduler scheduler, FixtureMappingManager methodFixtureMappings) =>
        new DependencyInjectionTestCaseRunner(context).Run(
            testCase,
            tests,
            messageBus,
            aggregator,
            cancellationTokenSource,
            parallelMode,
            scheduler,
            displayName,
            skipReason,
            explicitOption,
            constructorArguments,
            methodFixtureMappings);
}
