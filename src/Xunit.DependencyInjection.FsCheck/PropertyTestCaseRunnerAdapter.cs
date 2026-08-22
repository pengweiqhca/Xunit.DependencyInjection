using FsCheck.Xunit;
using Microsoft.FSharp.Control;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.DependencyInjection.FsCheck;

public class PropertyTestCaseRunnerAdapter : SimpleXunitTestCaseRunnerWrapper<PropertyTestCase>
{
    protected override ValueTask<RunSummary> RunAsync(IXunitTestCase testCase, IReadOnlyCollection<IXunitTest> tests,
        IMessageBus messageBus,
        ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, string displayName,
        string? skipReason, ExplicitOption explicitOption, object?[] constructorArguments, ParallelMode parallelMode,
        ExecutionScheduler scheduler, FixtureMappingManager methodFixtureMappings) =>
        new(FSharpAsync.StartAsTask(((PropertyTestCase)testCase).TestExec(explicitOption, messageBus,
            constructorArguments, aggregator, cancellationTokenSource, parallelMode, scheduler,
            methodFixtureMappings), null, new(cancellationTokenSource.Token)));
}
