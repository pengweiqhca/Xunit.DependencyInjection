namespace Xunit.DependencyInjection;

public class DependencyInjectionTestMethodRunner(DependencyInjectionTestContext context)
    : XunitTestMethodRunner
{
    // This method has been slightly modified from the original implementation to run tests in parallel
    // https://github.com/xunit/xunit/blob/2.4.2/src/xunit.execution/Sdk/Frameworks/Runners/TestMethodRunner.cs#L130-L142
    protected override async ValueTask<RunSummary> RunTestCases(XunitTestMethodRunnerContext ctxt, Exception? exception)
    {
        if (exception != null ||
            context.DisableParallelization ||
            ctxt.TestCases.Count < 2 ||
            ctxt.TestMethod.TestClass.Class.GetCustomAttribute<CollectionDefinitionAttribute>() is
            {
                DisableParallelization: true
            } ||
            ctxt.TestMethod.TestClass.Class.GetCustomAttribute<DisableParallelizationAttribute>() != null ||
            ctxt.TestMethod.TestClass.Class.GetCustomAttribute<CollectionAttribute>() != null &&
            !context.ForcedParallelization ||
            ctxt.TestMethod.Method.GetCustomAttribute<DisableParallelizationAttribute>() != null ||
            context is { ParallelSemaphore: not null, MaxParallelThreads: 1 })
        {
            if (context.ParallelSemaphore == null) return await base.RunTestCases(ctxt, exception);

            await context.ParallelSemaphore.WaitAsync(ctxt.CancellationTokenSource.Token);

            try
            {
                return await base.RunTestCases(ctxt, exception);
            }
            finally
            {
                context.ParallelSemaphore.Release();
            }
        }

        // Respect MaxParallelThreads by using the MaxConcurrencySyncContext if it exists, mimicking how collections are run
        // https://github.com/xunit/xunit/blob/v2-2.4.2/src/xunit.execution/Sdk/Frameworks/Runners/XunitTestAssemblyRunner.cs#L169-L176
        var scheduler = SynchronizationContext.Current == null
            ? TaskScheduler.Default
            : TaskScheduler.FromCurrentSynchronizationContext();

        Func<IXunitTestCase, Task<RunSummary>> taskRunner = context.ParallelSemaphore == null
            ? testCase => Task.Factory.StartNew(() => RunTestCase(ctxt, testCase).AsTask(),
                ctxt.CancellationTokenSource.Token,
                TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler,
                scheduler).Unwrap()
            : async testCase =>
            {
                await context.ParallelSemaphore.WaitAsync(ctxt.CancellationTokenSource.Token);

                try
                {
                    return await Task.Run(() => RunTestCase(ctxt, testCase).AsTask());
                }
                finally
                {
                    context.ParallelSemaphore.Release();
                }
            };

        var summary = new RunSummary();

        foreach (var caseSummary in await Task.WhenAll(ctxt.TestCases.Select(taskRunner)))
            summary.Aggregate(caseSummary);

        return summary;
    }

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
            ctxt.Aggregator.Add(ex);

            return base.RunTestCase(ctxt, testCase);
        }

        IXunitTestCaseRunnerWrapper? wrapper;
        var type = testCase.GetType();
        do
            wrapper = wrappers.FirstOrDefault(w => w.TestCaseType == type);
        while (wrapper == null && (type = type.BaseType) != null);

        return wrapper== null && testCase is ISelfExecutingXunitTestCase selfExecutingTestCase
            ? selfExecutingTestCase.Run(ctxt.ExplicitOption, ctxt.MessageBus, ctxt.ConstructorArguments,
                ctxt.Aggregator.Clone(), ctxt.CancellationTokenSource)
            : RunXunitTestCase(
                testCase,
                wrapper,
                ctxt.MessageBus,
                ctxt.CancellationTokenSource,
                ctxt.Aggregator.Clone(),
                ctxt.ExplicitOption,
                ctxt.ConstructorArguments);
    }

    private async ValueTask<RunSummary> RunXunitTestCase(IXunitTestCase testCase,
        IXunitTestCaseRunnerWrapper? adapter,
        IMessageBus messageBus,
        CancellationTokenSource cancellationTokenSource,
        ExceptionAggregator aggregator,
        ExplicitOption explicitOption,
        object?[] constructorArguments)
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
                    ex.Message.Substring(DynamicSkipToken.Value.Length),
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
                testCase.TestCaseDisplayName, testCase.SkipReason, explicitOption, constructorArguments);

        await using var scope = context.RootServices.CreateAsyncScope();

        context.RootServices.GetRequiredService<DependencyInjectionTypeActivator>().Services = scope.ServiceProvider;

        return await XunitTestCaseRunner.Instance.Run(
            testCase,
            tests,
            messageBus,
            aggregator,
            cancellationTokenSource,
            testCase.TestCaseDisplayName,
            testCase.SkipReason,
            explicitOption,
            constructorArguments);
    }
}
