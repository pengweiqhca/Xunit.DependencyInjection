namespace Xunit.DependencyInjection;

public class DependencyInjectionTestCollectionRunner : XunitTestCollectionRunner
{
    private readonly IServiceProvider _provider;
    private readonly IReadOnlyDictionary<ITestClass, IHost?> _hostMap;
    private IServiceScope? _serviceScope;
    private readonly IMessageSink _diagnosticMessageSink;

    public DependencyInjectionTestCollectionRunner(IServiceProvider provider,
        ITestCollection testCollection,
        IEnumerable<IXunitTestCase> testCases,
        IReadOnlyDictionary<ITestClass, IHost?> hostMap,
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        ITestCaseOrderer testCaseOrderer,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
        : base(testCollection, testCases, diagnosticMessageSink,
            messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
    {
        _provider = provider;
        _hostMap = hostMap;
        _diagnosticMessageSink = diagnosticMessageSink;
    }

    /// <inheritdoc />
    protected override void CreateCollectionFixture(Type fixtureType)
    {
        _serviceScope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();

        Aggregator.Run(() => CollectionFixtureMappings[fixtureType] =
            ActivatorUtilities.GetServiceOrCreateInstance(_serviceScope.ServiceProvider, fixtureType));
    }

    /// <inheritdoc/>
    protected override async Task BeforeTestCollectionFinishedAsync()
    {
        await base.BeforeTestCollectionFinishedAsync().ConfigureAwait(false);

        foreach (var fixture in CollectionFixtureMappings.Values.OfType<IAsyncDisposable>())
            await Aggregator.RunAsync(() => fixture.DisposeAsync().AsTask()).ConfigureAwait(false);

        if (_serviceScope != null) await _serviceScope.DisposeAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass,
        IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases) =>
        _hostMap.TryGetValue(testClass, out var host) && host != null
            ? new DependencyInjectionTestClassRunner(host.Services, testClass, @class, testCases,
                    _diagnosticMessageSink, MessageBus, TestCaseOrderer,
                    new ExceptionAggregator(Aggregator), CancellationTokenSource, CollectionFixtureMappings)
                .RunAsync()
            : base.RunTestClassAsync(testClass, @class, testCases);
}
