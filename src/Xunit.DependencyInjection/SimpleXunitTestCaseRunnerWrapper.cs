namespace Xunit.DependencyInjection;

public abstract class SimpleXunitTestCaseRunnerWrapper<T> : IXunitTestCaseRunnerWrapper
{
    public Type TestCaseType => typeof(T);

    public async ValueTask<RunSummary> RunAsync(DependencyInjectionContext context, IXunitTestCase testCase,
        IReadOnlyCollection<IXunitTest> tests,
        IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource,
        string displayName, string? skipReason, ExplicitOption explicitOption, object?[] constructorArguments,
        ParallelMode parallelMode, ExecutionScheduler scheduler, FixtureMappingManager methodFixtureMappings)
    {
        await using var scope = context.RootServices.CreateAsyncScope();

        if (FromServicesAttribute.CreateFromServices(testCase.TestMethod.Method).Count > 0)
            throw new NotSupportedException("Can't inject service via method arguments when use StaFact");

        context.RootServices.GetRequiredService<DependencyInjectionTypeActivator>().Services = scope.ServiceProvider;

        return await RunAsync(testCase, tests, messageBus, aggregator, cancellationTokenSource, displayName, skipReason,
            explicitOption, constructorArguments, parallelMode, scheduler, methodFixtureMappings);
    }

    protected abstract ValueTask<RunSummary> RunAsync(IXunitTestCase testCase, IReadOnlyCollection<IXunitTest> tests,
        IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource,
        string displayName, string? skipReason, ExplicitOption explicitOption, object?[] constructorArguments,
        ParallelMode parallelMode, ExecutionScheduler scheduler, FixtureMappingManager methodFixtureMappings);
}
