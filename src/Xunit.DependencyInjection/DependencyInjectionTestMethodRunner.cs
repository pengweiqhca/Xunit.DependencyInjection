namespace Xunit.DependencyInjection;

public class DependencyInjectionTestMethodRunner : TestMethodRunner<IXunitTestCase>
{
    private readonly IServiceProvider _provider;
    private readonly IMessageSink _diagnosticMessageSink;
    private readonly object?[] _constructorArguments;

    public DependencyInjectionTestMethodRunner(IServiceProvider provider,
        ITestMethod testMethod,
        IReflectionTypeInfo @class,
        IReflectionMethodInfo method,
        IEnumerable<IXunitTestCase> testCases,
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        object?[] constructorArguments)
        : base(testMethod, @class, method, testCases, messageBus, aggregator, cancellationTokenSource)
    {
        _provider = provider;
        _diagnosticMessageSink = diagnosticMessageSink;
        _constructorArguments = constructorArguments;
    }

    /// <inheritdoc />
    protected override async Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
    {
        if (testCase is ExecutionErrorTestCase)
            return await testCase.RunAsync(_diagnosticMessageSink, MessageBus, _constructorArguments,
                    new(Aggregator), CancellationTokenSource)
                .ConfigureAwait(false);

        var wrappers = _provider.GetServices<IXunitTestCaseRunnerWrapper>().Reverse().ToArray();

        var type = testCase.GetType();
        do
        {
            var adapter = wrappers.FirstOrDefault(w => w.TestCaseType == type);
            if (adapter != null)
                return await adapter.RunAsync(testCase, _provider, _diagnosticMessageSink, MessageBus,
                        _constructorArguments, new(Aggregator), CancellationTokenSource)
                    .ConfigureAwait(false);
        } while ((type = type.BaseType) != null);

        var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();

        await using (scope.ConfigureAwait(false))
            return await testCase.RunAsync(_diagnosticMessageSink, MessageBus,
                    ArgumentsHelper.CreateTestClassConstructorArguments(scope.ServiceProvider, _constructorArguments, Aggregator),
                    new(Aggregator), CancellationTokenSource)
                .ConfigureAwait(false);
    }
}
