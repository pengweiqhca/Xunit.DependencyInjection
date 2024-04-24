namespace Xunit.DependencyInjection;

public class DependencyInjectionTestCollectionRunner(
    DependencyInjectionStartupContext context,
    ITestCollection testCollection,
    IEnumerable<IXunitTestCase> testCases,
    IMessageSink diagnosticMessageSink,
    IMessageBus messageBus,
    ITestCaseOrderer testCaseOrderer,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource)
    : XunitTestCollectionRunner(testCollection, testCases, diagnosticMessageSink,
        messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
{
    private AsyncServiceScope? _serviceScope;

    /// <inheritdoc />
    protected override void CreateCollectionFixture(Type fixtureType)
    {
        if (context.DefaultRootServices is not { } provider)
        {
            Aggregator.Add(HostManager.MissingDefaultHost("Collection fixture  require a default startup."));

            base.CreateCollectionFixture(fixtureType);
        }
        else
        {
            var serviceScope = provider.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();

            _serviceScope = serviceScope;

            Aggregator.Run(() => CollectionFixtureMappings[fixtureType] =
                ActivatorUtilities.GetServiceOrCreateInstance(serviceScope.ServiceProvider, fixtureType));
        }
    }

    /// <inheritdoc/>
    protected override async Task BeforeTestCollectionFinishedAsync()
    {
        await base.BeforeTestCollectionFinishedAsync();

        foreach (var fixture in CollectionFixtureMappings.Values.OfType<IAsyncDisposable>())
            await Aggregator.RunAsync(() => fixture.DisposeAsync().AsTask());

        if (_serviceScope is { } disposable) await disposable.DisposeAsync();
    }

    /// <inheritdoc />
    protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass,
        IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases) =>
        context.ContextMap.TryGetValue(testClass, out var value) && value != null
            ? new DependencyInjectionTestClassRunner(
                new(value.Host, value.DisableParallelization ||
                    !(context.ParallelizationMode == ParallelizationMode.Force ||
                        context.ParallelizationMode == ParallelizationMode.Enhance &&
                        SynchronizationContext.Current is MaxConcurrencySyncContext),
                    context.ParallelizationMode == ParallelizationMode.Force), testClass, @class, testCases,
                DiagnosticMessageSink, MessageBus, TestCaseOrderer, new(Aggregator), CancellationTokenSource,
                CollectionFixtureMappings).RunAsync()
            : base.RunTestClassAsync(testClass, @class, testCases);
}
