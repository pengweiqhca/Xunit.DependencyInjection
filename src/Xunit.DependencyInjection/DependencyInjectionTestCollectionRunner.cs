namespace Xunit.DependencyInjection;

public class DependencyInjectionTestCollectionRunner(
    DependencyInjectionStartupContext context,
    ITestCollection testCollection,
    IEnumerable<IXunitTestCase> testCases,
    IMessageSink diagnosticMessageSink,
    IMessageBus messageBus,
    ITestCaseOrderer testCaseOrderer,
    ITestClassOrderer? testClassOrderer,
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

    protected override async Task<RunSummary> RunTestClassesAsync()
    {
        var summary = new RunSummary();

        IEnumerable<Tuple<ITestClass, List<IXunitTestCase>>> testClasses;

        if (testClassOrderer != null)
        {
            var dictionary = TestCases.GroupBy(tc => tc.TestMethod.TestClass, TestClassComparer.Instance)
                .ToDictionary(group => group.Key, group => group.ToList());

            IEnumerable<ITestClass> orderedTestCLasses;

            try
            {
                orderedTestCLasses = testClassOrderer.OrderTestClasses(dictionary.Keys);
            }
            catch (Exception ex)
            {
                var innerEx = ex.Unwrap();

                DiagnosticMessageSink.OnMessage(new DiagnosticMessage($"Test class orderer '{testClassOrderer.GetType().FullName}' threw '{innerEx.GetType().FullName}' during ordering: {innerEx.Message}{Environment.NewLine}{innerEx.StackTrace}"));

                orderedTestCLasses = dictionary.Keys;
            }

            testClasses = orderedTestCLasses.Select(collection => Tuple.Create(collection, dictionary[collection]));
        }
        else
        {
            testClasses = TestCases.GroupBy(tc => tc.TestMethod.TestClass, TestClassComparer.Instance).Select(group => Tuple.Create(group.Key, group.ToList()));
        }

        foreach (var testCasesByClass in testClasses)
        {
            summary.Aggregate(await RunTestClassAsync(testCasesByClass.Item1, (IReflectionTypeInfo)testCasesByClass.Item1.Class, testCasesByClass.Item2));

            if (CancellationTokenSource.IsCancellationRequested) break;
        }

        return summary;
    }

    /// <inheritdoc />
    protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass,
        IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases) =>
        context.ContextMap.TryGetValue(testClass, out var value) && value != null
            ? new DependencyInjectionTestClassRunner(
                new(value.Host, value.DisableParallelization ||
                    !(context.ParallelizationMode == ParallelizationMode.Force ||
                        context.ParallelizationMode == ParallelizationMode.Enhance &&
                        (SynchronizationContext.Current is MaxConcurrencySyncContext || context.ParallelSemaphore != null)),
                    context.ParallelizationMode == ParallelizationMode.Force, context.ParallelSemaphore), testClass, @class, testCases,
                DiagnosticMessageSink, MessageBus, TestCaseOrderer, new(Aggregator), CancellationTokenSource,
                CollectionFixtureMappings).RunAsync()
            : base.RunTestClassAsync(testClass, @class, testCases);
}
