using Xunit.Sdk;

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
            ctxt.TestMethod.Method.GetCustomAttributes<MemberDataAttribute>().Any(a => a.DisableDiscoveryEnumeration))
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
        // https://github.com/xunit/xunit/blob/2.4.2/src/xunit.execution/Sdk/Frameworks/Runners/XunitTestAssemblyRunner.cs#L169-L176
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
                    return await RunTestCase(ctxt, testCase);
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
        IXunitTestCase testCase) => testCase is ISelfExecutingXunitTestCase selfExecutingTestCase
        ? selfExecutingTestCase.Run(ctxt.ExplicitOption, ctxt.MessageBus, ctxt.ConstructorArguments,
            ctxt.Aggregator.Clone(), ctxt.CancellationTokenSource)
        : RunXunitTestCase(
            testCase,
            ctxt.MessageBus,
            ctxt.CancellationTokenSource,
            ctxt.Aggregator.Clone(),
            ctxt.ExplicitOption,
            ctxt.ConstructorArguments);

    private async ValueTask<RunSummary> RunXunitTestCase(IXunitTestCase testCase,
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

        return await RunXunitTestCase(testCase, tests, messageBus, cancellationTokenSource, aggregator, explicitOption,
            constructorArguments);
    }

    private ValueTask<RunSummary> RunXunitTestCase(IXunitTestCase testCase,
        IReadOnlyCollection<IXunitTest> tests,
        IMessageBus messageBus,
        CancellationTokenSource cancellationTokenSource,
        ExceptionAggregator aggregator,
        ExplicitOption explicitOption,
        object?[] constructorArguments)
    {
        IXunitTestCaseRunnerWrapper[] wrappers;
        try
        {
            wrappers = context.RootServices.GetServices<IXunitTestCaseRunnerWrapper>().Reverse().ToArray();
        }
        catch (Exception ex)
        {
            aggregator.Add(ex);

            return XunitTestCaseRunner.Instance.Run(
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

        var type = testCase.GetType();
        do
            if (wrappers.FirstOrDefault(w => w.TestCaseType == type) is { } adapter)
                return adapter.RunAsync(context, testCase, tests, messageBus, aggregator, cancellationTokenSource,
                    testCase.TestCaseDisplayName, testCase.SkipReason, explicitOption, constructorArguments);
        while ((type = type.BaseType) != null);

        return BaseRun(context.RootServices.CreateAsyncScope());

        async ValueTask<RunSummary> BaseRun(AsyncServiceScope scope)
        {
            await using (scope)
                return await XunitTestCaseRunner.Instance.Run(
                    testCase,
                    tests,
                    messageBus,
                    aggregator,
                    cancellationTokenSource,
                    testCase.TestCaseDisplayName,
                    testCase.SkipReason,
                    explicitOption,
                    scope.ServiceProvider.CreateTestClassConstructorArguments(constructorArguments, aggregator));
        }
    }
}
