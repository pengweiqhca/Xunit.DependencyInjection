namespace Xunit.DependencyInjection;

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
            TestMethod.TestClass.Class.GetCustomAttributes(typeof(CollectionDefinitionAttribute)).FirstOrDefault() is { } attr &&
            attr.GetNamedArgument<bool>(nameof(CollectionDefinitionAttribute.DisableParallelization)) ||
            TestMethod.TestClass.Class.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any() ||
            TestMethod.TestClass.Class.GetCustomAttributes(typeof(CollectionAttribute)).Any() && !context.ForcedParallelization ||
            TestMethod.Method.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any() ||
            TestMethod.Method.GetCustomAttributes(typeof(MemberDataAttribute)).Any(a =>
                a.GetNamedArgument<bool>(nameof(MemberDataAttribute.DisableDiscoveryEnumeration))))
            return await base.RunTestCasesAsync();

        // Respect MaxParallelThreads by using the MaxConcurrencySyncContext if it exists, mimicking how collections are run
        // https://github.com/xunit/xunit/blob/2.4.2/src/xunit.execution/Sdk/Frameworks/Runners/XunitTestAssemblyRunner.cs#L169-L176
        var scheduler = SynchronizationContext.Current == null
            ? TaskScheduler.Default
            : TaskScheduler.FromCurrentSynchronizationContext();

        var tasks = TestCases.Select(testCase => Task.Factory.StartNew(
            state => RunTestCaseAsync((IXunitTestCase)state!), testCase, CancellationTokenSource.Token,
            TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap());

        var summary = new RunSummary();

        foreach (var caseSummary in await Task.WhenAll(tasks))
            summary.Aggregate(caseSummary);

        return summary;
    }

    /// <inheritdoc />
    protected override Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
    {
        if (testCase is ExecutionErrorTestCase)
            return testCase.RunAsync(diagnosticMessageSink, MessageBus, constructorArguments,
                new(Aggregator), CancellationTokenSource);

        var wrappers = context.RootServices.GetServices<IXunitTestCaseRunnerWrapper>().Reverse().ToArray();

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
