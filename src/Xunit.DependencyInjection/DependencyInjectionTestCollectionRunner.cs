namespace Xunit.DependencyInjection;

public class DependencyInjectionTestCollectionRunner : XunitTestCollectionRunner
{
    private readonly DependencyInjectionStartupContext _context;
    private readonly IMessageSink _diagnosticMessageSink;
    private AsyncServiceScope? _serviceScope;

    public DependencyInjectionTestCollectionRunner(DependencyInjectionStartupContext context,
        ITestCollection testCollection,
        IEnumerable<IXunitTestCase> testCases,
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        ITestCaseOrderer testCaseOrderer,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
        : base(testCollection, testCases, diagnosticMessageSink,
            messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
    {
        _context = context;
        _diagnosticMessageSink = diagnosticMessageSink;
    }

    /// <inheritdoc />
    protected override void CreateCollectionFixture(Type fixtureType)
    {
        if (_context.DefaultRootServices is not { } provider)
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
        await base.BeforeTestCollectionFinishedAsync().ConfigureAwait(false);

        foreach (var fixture in CollectionFixtureMappings.Values.OfType<IAsyncDisposable>())
            await Aggregator.RunAsync(() => fixture.DisposeAsync().AsTask()).ConfigureAwait(false);

        if (_serviceScope is { } disposable) await disposable.DisposeAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass,
        IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases) =>
        _context.ContextMap.TryGetValue(testClass, out var context) && context != null
            ? new DependencyInjectionTestClassRunner(
                new(context.Host, context.DisableParallelization ||
                    !(_context.ParallelizationMode == ParallelizationMode.Force ||
                        _context.ParallelizationMode == ParallelizationMode.Enhance &&
                        SynchronizationContext.Current is MaxConcurrencySyncContext)), testClass, @class, testCases,
                _diagnosticMessageSink, MessageBus, TestCaseOrderer, new(Aggregator), CancellationTokenSource,
                CollectionFixtureMappings).RunAsync()
            : base.RunTestClassAsync(testClass, @class, testCases);
}
