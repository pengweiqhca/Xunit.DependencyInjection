namespace Xunit.DependencyInjection;

public class DependencyInjectionTestMethodRunner(DependencyInjectionContext context)
    : XunitTestMethodRunner
{
    /// <inheritdoc />
    protected override ValueTask<RunSummary> RunTestCase(XunitTestMethodRunnerContext ctxt,
        IXunitTestCase testCase)
    {
        IXunitTestCaseRunnerWrapper[] wrappers;
        try
        {
            wrappers = [.. context.RootServices.GetServices<IXunitTestCaseRunnerWrapper>().Reverse()];
        }
        catch (Exception ex)
        {
            ctxt.Aggregator.Add(context.Host.Services.GetService<IAsyncExceptionFilter>()?.Process(ex) ?? ex);

            return base.RunTestCase(ctxt, testCase);
        }

        IXunitTestCaseRunnerWrapper? wrapper;
        var type = testCase.GetType();
        do
            wrapper = wrappers.FirstOrDefault(w => w.TestCaseType == type);
        while (wrapper == null && (type = type.BaseType) != null);

        return wrapper== null && testCase is ISelfExecutingXunitTestCase selfExecutingTestCase
            ? selfExecutingTestCase.Run(ctxt.ExplicitOption, ctxt.MessageBus, ctxt.ConstructorArguments,
                ctxt.Aggregator.Clone(), ctxt.CancellationTokenSource, ctxt.ParallelMode, ctxt.Scheduler,
                ctxt.MethodFixtureMappings)
            : RunXunitTestCase(
                testCase,
                wrapper,
                ctxt.MessageBus,
                ctxt.CancellationTokenSource,
                ctxt.Aggregator.Clone(),
                ctxt.ExplicitOption,
                ctxt.ConstructorArguments,
                ctxt.ParallelMode,
                ctxt.Scheduler,
                ctxt.MethodFixtureMappings);
    }

    private async ValueTask<RunSummary> RunXunitTestCase(IXunitTestCase testCase,
        IXunitTestCaseRunnerWrapper? adapter,
        IMessageBus messageBus,
        CancellationTokenSource cancellationTokenSource,
        ExceptionAggregator aggregator,
        ExplicitOption explicitOption,
        object?[] constructorArguments,
        ParallelMode parallelMode,
        ExecutionScheduler scheduler,
        FixtureMappingManager methodFixtureMappings)
    {
        IAsyncDisposable? disposable = default;

        if (testCase is XunitDelayEnumeratedTheoryTestCase &&
            testCase.TestMethod.DataAttributes.OfType<MethodDataAttribute>().Any())
        {
            disposable = TheoryTestCaseDataContext.BeginContext(context.RootServices);
        }

        await using var _ = disposable;

        var tests = await aggregator.RunAsync(testCase.CreateTests, []);

        if (aggregator.ToException() is { } ex)
        {
            if (ex.Message.StartsWith(DynamicSkipToken.Value, StringComparison.Ordinal))
                return XunitRunnerHelper.SkipTestCases(
                    messageBus,
                    cancellationTokenSource,
                    [testCase],
                    ex.Message[DynamicSkipToken.Value.Length..],
                    sendTestCaseMessages: false
                );

            return XunitRunnerHelper.FailTestCases(
                messageBus,
                cancellationTokenSource,
                [testCase],
                ex,
                sendTestCaseMessages: false
            );
        }

        if (adapter != null)
            return await adapter.RunAsync(context, testCase, tests, messageBus, aggregator, cancellationTokenSource,
                testCase.TestCaseDisplayName, testCase.SkipReason, explicitOption, constructorArguments, parallelMode,
                scheduler, methodFixtureMappings);

        await using var scope = context.RootServices.CreateAsyncScope();

        context.RootServices.GetRequiredService<DependencyInjectionTypeActivator>().Services = scope.ServiceProvider;

        return await XunitTestCaseRunner.Instance.Run(
            testCase,
            tests,
            messageBus,
            aggregator,
            cancellationTokenSource,
            parallelMode,
            scheduler,
            testCase.TestCaseDisplayName,
            testCase.SkipReason,
            explicitOption,
            constructorArguments,
            methodFixtureMappings);
    }
}
