using Xunit.Internal;

namespace Xunit.DependencyInjection;

public class DependencyInjectionTestCollectionRunner(
    DependencyInjectionStartupContext context,
    ITestClassOrderer? testClassOrderer)
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
    /// <param name="testCaseOrderer">The test case orderer that was applied at the assembly level.</param>
    /// <param name="aggregator">The exception aggregator used to run code and collection exceptions.</param>
    /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
    /// <param name="assemblyFixtureMappings">The mapping manager for assembly fixtures.</param>
    public async ValueTask<RunSummary> Run(IXunitTestCollection testCollection,
        IReadOnlyCollection<IXunitTestCase> testCases,
        ExplicitOption explicitOption,
        IMessageBus messageBus,
        ITestCaseOrderer testCaseOrderer,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        FixtureMappingManager assemblyFixtureMappings)
    {
        await using var ctxt = new DependencyInjectionTestCollectionRunnerContext(
            new(testCollection), OrderTestClass(testCases, messageBus).CastOrToReadOnlyCollection(), explicitOption,
            messageBus, testCaseOrderer, aggregator, cancellationTokenSource, assemblyFixtureMappings);

        await ctxt.InitializeAsync();

        return await Run(ctxt);
    }

    private IEnumerable<IXunitTestCase> OrderTestClass(IReadOnlyCollection<IXunitTestCase> testCases, IMessageBus messageBus)
    {
        if (testClassOrderer == null) return testCases;

        var dictionary = testCases.GroupBy(tc => tc.TestMethod.TestClass, TestClassComparer.Instance)
            .ToDictionary(group => group.Key!, group => group.ToList());

        IEnumerable<ITestClass> orderedTestCLasses;

        try
        {
            orderedTestCLasses = testClassOrderer.OrderTestClasses(dictionary.Keys);
        }
        catch (Exception ex)
        {
            var innerEx = ex.Unwrap();

            messageBus.QueueMessage(new DiagnosticMessage(
                $"Test class orderer '{testClassOrderer.GetType().FullName}' threw '{innerEx.GetType().FullName}' during ordering: {innerEx.Message}{Environment.NewLine}{innerEx.StackTrace}"));

            return testCases;
        }

        return orderedTestCLasses.SelectMany(testCLass =>
            dictionary.TryGetValue(testCLass, out var testCasesList) ? testCasesList : []);
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

        var testClassRunner = context.ContextMap.TryGetValue(testClass, out var value) && value != null
            ? new DependencyInjectionTestClassRunner(
                new(value.Host, value.DisableParallelization ||
                    !(context.ParallelizationMode == ParallelizationMode.Force ||
                        context.ParallelizationMode == ParallelizationMode.Enhance &&
                        (SynchronizationContext.Current is MaxConcurrencySyncContext ||
                            context.ParallelSemaphore != null)),
                    context.ParallelizationMode == ParallelizationMode.Force,
                    context.MaxParallelThreads, context.ParallelSemaphore))
            : XunitTestClassRunner.Instance;

        return testClassRunner.Run(
            testClass,
            testCases,
            ctxt.ExplicitOption,
            ctxt.MessageBus,
            ctxt.TestCaseOrderer,
            ctxt.Aggregator.Clone(),
            ctxt.CancellationTokenSource,
            ctxt.CollectionFixtureMappings);
    }
}
public class DependencyInjectionTestCollectionRunnerContext(
    DependencyInjectionTestCollection testCollection,
    IReadOnlyCollection<IXunitTestCase> testCases,
    ExplicitOption explicitOption,
    IMessageBus messageBus,
    ITestCaseOrderer testCaseOrderer,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource,
    FixtureMappingManager assemblyFixtureMappings) :
    XunitTestCollectionRunnerBaseContext<DependencyInjectionTestCollection, IXunitTestCase>(testCollection, testCases, explicitOption, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, assemblyFixtureMappings)
{ }

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

    IReadOnlyCollection<Type> IXunitTestCollection.ClassFixtureTypes { get; } = testCollection.CollectionFixtureTypes
        .Where(TestHelper.GenericTypeArgumentIsGenericParameter).ToArray();

    public IReadOnlyCollection<Type> CollectionFixtureTypes { get; } = testCollection.CollectionFixtureTypes
        .WhereNot(TestHelper.GenericTypeArgumentIsGenericParameter).ToArray();

    public Type? CollectionDefinition => testCollection.CollectionDefinition;

    public bool DisableParallelization => testCollection.DisableParallelization;

    public IXunitTestAssembly TestAssembly => testCollection.TestAssembly;

    public ITestCaseOrderer? TestCaseOrderer => testCollection.TestCaseOrderer;

    ITestAssembly ITestCollection.TestAssembly => TestAssembly;
}
