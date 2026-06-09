using FsCheck.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.DependencyInjection.FsCheck;

public class PropertyTestCaseRunnerAdapter : SimpleXunitTestCaseRunnerWrapper<PropertyTestCase>
{
    protected override ValueTask<RunSummary> RunAsync(IXunitTestCase testCase,
        IReadOnlyCollection<IXunitTest> tests, IMessageBus messageBus, ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        string displayName, string? skipReason, ExplicitOption explicitOption, object?[] constructorArguments) =>
        new(FSharpAsync.StartAsTask(((PropertyTestCase)testCase).TestExec(explicitOption, messageBus,
            constructorArguments, aggregator, cancellationTokenSource), null, new(cancellationTokenSource.Token)));
}
