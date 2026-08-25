using xRetry.v3;
using Xunit.Sdk;
using Xunit.v3;

#pragma warning disable IDE1006 // Naming Styles
namespace Xunit.DependencyInjection.xRetry;
#pragma warning restore IDE1006 // Naming Styles

public class RetryTestCaseRunnerWrapper<T> : SimpleXunitTestCaseRunnerWrapper<T>
    where T : IRetryableTestCase
{
    /// <inheritdoc />
    protected override ValueTask<RunSummary> RunAsync(IXunitTestCase testCase, IReadOnlyCollection<IXunitTest> tests,
        IMessageBus messageBus,
        ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, string displayName,
        string? skipReason, ExplicitOption explicitOption, object?[] constructorArguments, ParallelMode parallelMode,
        ExecutionScheduler scheduler, FixtureMappingManager methodFixtureMappings) =>
        RetryTestCaseRunner.Instance.Run((IRetryableTestCase)testCase, tests, messageBus, aggregator,
            cancellationTokenSource, displayName, skipReason,
            explicitOption, constructorArguments, parallelMode, scheduler, methodFixtureMappings);
}
