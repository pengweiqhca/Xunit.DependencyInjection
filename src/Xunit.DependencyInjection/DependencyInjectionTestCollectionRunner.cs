namespace Xunit.DependencyInjection;

public class DependencyInjectionTestCollectionRunner(
    DependencyInjectionStartupContext context)
    : XunitTestCollectionRunnerBase<DependencyInjectionTestCollectionRunnerContext, DependencyInjectionTestCollection, IXunitTestClass,
        IXunitTestCase>
{
    private AsyncServiceScope? _serviceScope;

    /// <summary>
    /// Runs the test collection.
    /// </summary>
    /// <param name="testCollection">The test collection to be run.</param>
    /// <param name="testCases">The test cases to be run. Cannot be empty.</param>
    /// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
    /// <param name="messageBus">The message bus to report run status to.</param>
    /// <param name="aggregator">The exception aggregator used to run code and collection exceptions.</param>
    /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
    /// <param name="parallelMode">The parallel mode for the test collection.</param>
    /// <param name="scheduler">The scheduler used for task/test scheduling.</param>
    /// <param name="assemblyFixtureMappings">The mapping manager for assembly fixtures.</param>
    public async ValueTask<RunSummary> Run(IXunitTestCollection testCollection,
        IReadOnlyCollection<IXunitTestCase> testCases,
        ExplicitOption explicitOption,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        ParallelMode parallelMode,
        ExecutionScheduler scheduler,
        FixtureMappingManager assemblyFixtureMappings)
    {
        await using var ctxt = new DependencyInjectionTestCollectionRunnerContext(
            new(testCollection), testCases, explicitOption,
            messageBus, aggregator, cancellationTokenSource, parallelMode, scheduler, assemblyFixtureMappings);

        await ctxt.InitializeAsync();

        return await Run(ctxt);
    }

    /// <inheritdoc />
    protected override async ValueTask<bool> OnTestCollectionStarting(DependencyInjectionTestCollectionRunnerContext ctxt)
    {
        if (ctxt.TestCollection.CollectionFixtureTypes.Count > 0)
        {
            if (context.DefaultRootServices is not { } provider)
            {
                ctxt.Aggregator.Add(HostManager.MissingDefaultHost("Collection fixture require a default startup."));
            }
            else
            {
                var serviceScope = provider.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();

                _serviceScope = serviceScope;

                await ctxt.CollectionFixtureMappings.CreateFixtures(ctxt.TestCollection.CollectionFixtureTypes,
                    ctxt.Aggregator, serviceScope.ServiceProvider);
            }
        }

        return await base.OnTestCollectionStarting(ctxt);
    }

    /// <inheritdoc/>
    protected override async ValueTask<bool> OnTestCollectionFinished(DependencyInjectionTestCollectionRunnerContext ctxt,
        RunSummary summary)
    {
        if (_serviceScope is not { } disposable)
            return await base.OnTestCollectionFinished(ctxt, summary);

        try
        {
            ctxt.CollectionFixtureMappings.ClearFixtures(ctxt.TestCollection.CollectionFixtureTypes, disposable.ServiceProvider);

            return await base.OnTestCollectionFinished(ctxt, summary);
        }
        finally
        {
            await disposable.DisposeAsync();
        }
    }

    protected override ValueTask<RunSummary> RunTestClass(DependencyInjectionTestCollectionRunnerContext ctxt,
        IXunitTestClass? testClass, IReadOnlyCollection<IXunitTestCase> testCases)
    {
        if (testClass is null)
            return new(XunitRunnerHelper.FailTestCases(
                ctxt.MessageBus,
                ctxt.CancellationTokenSource,
                testCases,
                "Test case '{0}' does not have an associated class and cannot be run by XunitTestClassRunner",
                sendTestClassMessages: true,
                sendTestMethodMessages: true
            ));

        var testClassRunner = context.ContextMap.TryGetValue(testClass, out var value) && value is { Disposed: false }
            ? new DependencyInjectionTestClassRunner(
                new(value.Host, IsParallelizationDisabled(testClass, ctxt)))
            : XunitTestClassRunner.Instance;

        return testClassRunner.Run(
            testClass,
            testCases,
            ctxt.ExplicitOption,
            ctxt.MessageBus,
            ctxt.Aggregator.Clone(),
            ctxt.CancellationTokenSource,
            IsParallelizationDisabled(testClass, ctxt) ? ParallelMode.None : ParallelMode.All,
            ctxt.Scheduler,
            ctxt.CollectionFixtureMappings);
    }

    private bool IsParallelizationDisabled(IXunitTestClass testClass,
        DependencyInjectionTestCollectionRunnerContext ctxt) =>
        ctxt.ParallelMode == ParallelMode.None ||
        testClass.Class.GetCustomAttribute<DisableParallelizationAttribute>() is not null ||
        context.ContextMap.TryGetValue(testClass, out var value) && value is { DisableParallelization: true };
}
public class DependencyInjectionTestCollectionRunnerContext(
    DependencyInjectionTestCollection testCollection,
    IReadOnlyCollection<IXunitTestCase> testCases,
    ExplicitOption explicitOption,
    IMessageBus messageBus,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource,
    ParallelMode parallelMode,
    ExecutionScheduler scheduler,
    FixtureMappingManager assemblyFixtureMappings) :
    XunitTestCollectionRunnerBaseContext<DependencyInjectionTestCollection, IXunitTestClass, IXunitTestCase>(
        testCollection, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, parallelMode, scheduler,
        assemblyFixtureMappings)
{
    public override ValueTask<RunSummary> RunTestClass(IXunitTestClass testClass,
        IReadOnlyCollection<IXunitTestCase> testCases) =>
        XunitTestClassRunner.Instance.Run(
            testClass,
            testCases,
            ExplicitOption,
            MessageBus,
            Aggregator.Clone(),
            CancellationTokenSource,
            ParallelMode,
            Scheduler,
            CollectionFixtureMappings);
}

#pragma warning disable CA1711
public sealed class DependencyInjectionTestCollection(IXunitTestCollection testCollection) : IXunitTestCollection
#pragma warning restore CA1711
{
    public string? TestCollectionClassName => testCollection.TestCollectionClassName;

    public string TestCollectionDisplayName => testCollection.TestCollectionDisplayName;

    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits => testCollection.Traits;

    public string UniqueID => testCollection.UniqueID;

    public IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes =>
        testCollection.BeforeAfterTestAttributes;

    IReadOnlyCollection<Type> IXunitTestCollection.ClassFixtureTypes { get; } =
    [
        .. testCollection.CollectionFixtureTypes
            .Where(TestHelper.GenericTypeArgumentIsGenericParameter)
    ];

    public IReadOnlyCollection<Type> CollectionFixtureTypes { get; } =
    [
        .. testCollection.CollectionFixtureTypes
            .WhereNot(TestHelper.GenericTypeArgumentIsGenericParameter)
    ];

    public Type? CollectionDefinition => testCollection.CollectionDefinition;

    public bool DisableParallelization => testCollection.DisableParallelization;

    public IXunitTestAssembly TestAssembly => testCollection.TestAssembly;

    public ITestCaseOrderer? TestCaseOrderer => testCollection.TestCaseOrderer;

    public ITestClassOrderer? TestClassOrderer => testCollection.TestClassOrderer;

    public ITestMethodOrderer? TestMethodOrderer => testCollection.TestMethodOrderer;

    ICoreTestAssembly ICoreTestCollection.TestAssembly => TestAssembly;

    ITestAssembly ITestCollection.TestAssembly => TestAssembly;
}
