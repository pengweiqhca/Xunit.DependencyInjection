using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.DependencyInjection.StaFact;

// ReSharper disable once InconsistentNaming
public class UITestCaseRunnerAdapter : SimpleXunitTestCaseRunnerWrapper<UITestCase>
{
    protected override ValueTask<RunSummary> RunAsync(IXunitTestCase testCase, IReadOnlyCollection<IXunitTest> tests, IMessageBus messageBus,
        ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, string displayName,
        string? skipReason, ExplicitOption explicitOption, object?[] constructorArguments, ParallelMode parallelMode,
        ExecutionScheduler scheduler, FixtureMappingManager methodFixtureMappings) =>
        ((UITestCase)testCase).Run(explicitOption, messageBus, constructorArguments, aggregator,
            cancellationTokenSource);
}

public class UITheoryTestCaseRunnerAdapter : SimpleXunitTestCaseRunnerWrapper<UIDelayEnumeratedTestCase>
{
    protected override ValueTask<RunSummary> RunAsync(IXunitTestCase testCase, IReadOnlyCollection<IXunitTest> tests, IMessageBus messageBus,
        ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, string displayName,
        string? skipReason, ExplicitOption explicitOption, object?[] constructorArguments, ParallelMode parallelMode,
        ExecutionScheduler scheduler, FixtureMappingManager methodFixtureMappings) =>
        ((UIDelayEnumeratedTestCase)testCase).Run(explicitOption, messageBus, constructorArguments, aggregator,
            cancellationTokenSource);
}
