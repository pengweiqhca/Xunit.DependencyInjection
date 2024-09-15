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

    protected override async Task<RunSummary> RunTestClassesAsync()
    {
        var groups = TestCases
            .GroupBy(tc => tc.TestMethod.TestClass);

        var selected = groups.Select(g => new
        {
            TestClass = g.Key,
            ReflectionTypeInfo = g.Key.Class as IReflectionTypeInfo ?? throw new ArgumentException($"RunTestClassesAsync - Could not cast class to IReflectionTypeInfo for class name = {g.Key.Class.Name}"),
            TestCases = g.AsEnumerable()
        });

        var orderedTestClasses = selected.OrderBy(tc =>
        {
            var classOrderAttribute = tc.TestClass.Class.GetCustomAttributes(typeof(TestClassOrderAttribute)).FirstOrDefault();
            return classOrderAttribute == null ? int.MaxValue : classOrderAttribute.GetNamedArgument<int>("Order");
        });

        var runSummary = new RunSummary();
        foreach (var testClass in orderedTestClasses)
        {
            runSummary.Aggregate(await RunTestClassAsync(testClass.TestClass, testClass.ReflectionTypeInfo, testClass.TestCases));
        }

        return runSummary;
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
