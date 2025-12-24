using Microsoft.Extensions.DependencyInjection;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.DependencyInjection.StaFact;

// ReSharper disable once InconsistentNaming
public class UITestCaseRunnerAdapter : IXunitTestCaseRunnerWrapper
{
    /// <inheritdoc />
    public virtual Type TestCaseType => typeof(UITestCase);

    public async ValueTask<RunSummary> RunAsync(DependencyInjectionContext context, IXunitTestCase testCase, IReadOnlyCollection<IXunitTest> tests,
        IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource,
        string displayName, string? skipReason, ExplicitOption explicitOption, object?[] constructorArguments)
    {
        await using var scope = context.RootServices.CreateAsyncScope();

        if (FromServicesAttribute.CreateFromServices(testCase.TestMethod.Method).Count > 0)
            throw new NotSupportedException("Can't inject service via method arguments when use StaFact");

        context.RootServices.GetRequiredService<DependencyInjectionTypeActivator>().Services = scope.ServiceProvider;

        return await ((UITestCase)testCase).Run(explicitOption, messageBus, constructorArguments, aggregator,
            cancellationTokenSource);
    }
}

public class UITheoryTestCaseRunnerAdapter : UITestCaseRunnerAdapter
{
    /// <inheritdoc />
    public override Type TestCaseType => typeof(UIDelayEnumeratedTestCase);
}
