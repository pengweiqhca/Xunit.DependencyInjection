﻿namespace Xunit.DependencyInjection;

public class DependencyInjectionTestMethodRunner(
    DependencyInjectionTestContext context,
    ITestMethod testMethod,
    IReflectionTypeInfo @class,
    IReflectionMethodInfo method,
    IEnumerable<IXunitTestCase> testCases,
    IMessageSink diagnosticMessageSink,
    IMessageBus messageBus,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource,
    object?[] constructorArguments)
    : TestMethodRunner<IXunitTestCase>(testMethod, @class, method, testCases, messageBus, aggregator,
        cancellationTokenSource)
{
    // This method has been slightly modified from the original implementation to run tests in parallel
    // https://github.com/xunit/xunit/blob/2.4.2/src/xunit.execution/Sdk/Frameworks/Runners/TestMethodRunner.cs#L130-L142
    protected override async Task<RunSummary> RunTestCasesAsync()
    {
        if (context.DisableParallelization ||
            TestCases.Count() < 2 ||
            TestMethod.TestClass.Class.GetCustomAttributes(typeof(CollectionDefinitionAttribute)).FirstOrDefault() is
                { } attr &&
            attr.GetNamedArgument<bool>(nameof(CollectionDefinitionAttribute.DisableParallelization)) ||
            TestMethod.TestClass.Class.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any() ||
            TestMethod.TestClass.Class.GetCustomAttributes(typeof(CollectionAttribute)).Any() &&
            !context.ForcedParallelization ||
            TestMethod.Method.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any() ||
            context is { ParallelSemaphore: not null, MaxParallelThreads: 1 })
        {
            if (context.ParallelSemaphore is not null)
                await context.ParallelSemaphore.WaitAsync(CancellationTokenSource.Token);

            try
            {
                return await base.RunTestCasesAsync();
            }
            finally
            {
                context.ParallelSemaphore?.Release();
            }
        }

        // Respect MaxParallelThreads by using the MaxConcurrencySyncContext if it exists, mimicking how collections are run
        // https://github.com/xunit/xunit/blob/v2-2.4.2/src/xunit.execution/Sdk/Frameworks/Runners/XunitTestAssemblyRunner.cs#L169-L176
        var scheduler = SynchronizationContext.Current == null
            ? TaskScheduler.Default
            : TaskScheduler.FromCurrentSynchronizationContext();

        Func<IXunitTestCase, Task<RunSummary>> taskRunner = context.ParallelSemaphore == null
            ? testCase => Task.Factory.StartNew(state => RunTestCaseAsync((IXunitTestCase)state!), testCase,
                CancellationTokenSource.Token, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler,
                scheduler).Unwrap()
            : async testCase =>
            {
                await context.ParallelSemaphore.WaitAsync(CancellationTokenSource.Token).ConfigureAwait(false);

                try
                {
                    return await Task.Run(() => RunTestCaseAsync(testCase)).ConfigureAwait(false);
                }
                finally
                {
                    context.ParallelSemaphore.Release();
                }
            };

        var summary = new RunSummary();

        foreach (var caseSummary in await Task.WhenAll(TestCases.Select(taskRunner)))
            summary.Aggregate(caseSummary);

        return summary;
    }

    /// <inheritdoc />
    protected override Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
    {
        if (testCase is ExecutionErrorTestCase)
            return testCase.RunAsync(diagnosticMessageSink, MessageBus, constructorArguments,
                new(Aggregator), CancellationTokenSource);

        IXunitTestCaseRunnerWrapper[] wrappers;
        try
        {
            wrappers = [.. context.RootServices.GetServices<IXunitTestCaseRunnerWrapper>().Reverse()];
        }
        catch (Exception ex)
        {
            Aggregator.Add(ex);

            return testCase.RunAsync(diagnosticMessageSink, MessageBus, constructorArguments,
                new(Aggregator), CancellationTokenSource);
        }

        var type = testCase.GetType();
        do
            if (wrappers.FirstOrDefault(w => w.TestCaseType == type) is { } adapter)
                return adapter.RunAsync(testCase, context, diagnosticMessageSink, MessageBus,
                    constructorArguments, new(Aggregator), CancellationTokenSource);
        while ((type = type.BaseType) != null);

        return BaseRun(context.RootServices.CreateAsyncScope());

        async Task<RunSummary> BaseRun(AsyncServiceScope scope)
        {
            await using (scope)
                return await testCase.RunAsync(diagnosticMessageSink, MessageBus,
                    scope.ServiceProvider.CreateTestClassConstructorArguments(constructorArguments, Aggregator),
                    new(Aggregator), CancellationTokenSource);
        }
    }
}
