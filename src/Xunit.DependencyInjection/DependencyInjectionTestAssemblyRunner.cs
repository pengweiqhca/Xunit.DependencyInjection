namespace Xunit.DependencyInjection;

public class DependencyInjectionTestAssemblyRunner(
    DependencyInjectionStartupContext context,
    IReadOnlyCollection<Exception> exceptions)
    : XunitTestAssemblyRunnerBase<DependencyInjectionAssemblyRunnerContext, DependencyInjectionTestAssembly,
        IXunitTestCollection,
        IXunitTestCase>
{
    protected override async ValueTask<bool> OnTestAssemblyStarting(DependencyInjectionAssemblyRunnerContext ctxt)
    {
        if (exceptions.Count > 0)
        {
            foreach (var ex in exceptions)
                ctxt.Aggregator.Add(ex);
        }
        else if (ctxt.TestAssembly.AssemblyFixtureTypes.Count > 0)
        {
            if (context.DefaultRootServices == null)
                ctxt.Aggregator.Add(HostManager.MissingDefaultHost("Assembly fixture require a default startup."));
            else
                await ctxt.AssemblyFixtureMappings.CreateFixtures(ctxt.TestAssembly.AssemblyFixtureTypes,
                    ctxt.Aggregator, context.DefaultRootServices);
        }

        return await base.OnTestAssemblyStarting(ctxt);
    }

    protected override ValueTask<bool> OnTestAssemblyFinished(DependencyInjectionAssemblyRunnerContext ctxt,
        RunSummary summary)
    {
        if (context.DefaultRootServices != null)
            ctxt.AssemblyFixtureMappings.ClearFixtures(ctxt.TestAssembly.AssemblyFixtureTypes,
                context.DefaultRootServices);

        return base.OnTestAssemblyFinished(ctxt, summary);
    }

    /// <summary>
    /// Runs the test assembly.
    /// </summary>
    /// <param name="testAssembly">The test assembly to be executed.</param>
    /// <param name="testCases">The test cases associated with the test assembly.</param>
    /// <param name="executionMessageSink">The message sink to send execution messages to.</param>
    /// <param name="executionOptions">The execution options to use when running tests.</param>
    public async ValueTask<RunSummary> Run(DependencyInjectionTestAssembly testAssembly,
        IReadOnlyCollection<IXunitTestCase> testCases,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions)
    {
        await using var ctxt = new DependencyInjectionAssemblyRunnerContext(context, testAssembly,
            testCases, executionMessageSink, executionOptions);

        await ctxt.InitializeAsync();

        return await Run(ctxt);
    }

    /// <inheritdoc />
    protected override ValueTask<RunSummary> RunTestCollection(DependencyInjectionAssemblyRunnerContext ctxt,
        IXunitTestCollection testCollection,
        IReadOnlyCollection<IXunitTestCase> testCases) =>
        ctxt.RunTestCollection(testCollection, testCases,
            ctxt.AssemblyTestCaseOrderer ?? DefaultTestCaseOrderer.Instance);
}

public class DependencyInjectionAssemblyRunnerContext(
    DependencyInjectionStartupContext context,
    DependencyInjectionTestAssembly testAssembly,
    IReadOnlyCollection<IXunitTestCase> testCases,
    IMessageSink executionMessageSink,
    ITestFrameworkExecutionOptions executionOptions)
    : XunitTestAssemblyRunnerBaseContext<DependencyInjectionTestAssembly, IXunitTestCase>(testAssembly, testCases,
        executionMessageSink, executionOptions)
{
    public override void SetupParallelism()
    {
        base.SetupParallelism();

        if (ParallelAlgorithm != default) return;

        context.ParallelSemaphore ??=
            typeof(XunitTestAssemblyRunnerBaseContext<DependencyInjectionTestAssembly, IXunitTestCase>)
                .GetField("parallelSemaphore", BindingFlags.Instance | BindingFlags.NonPublic)?
                .GetValue(this) as SemaphoreSlim;
    }

    /// <summary>
    /// Delegation of <see cref="XunitTestAssemblyRunnerBase{TContext, TTestAssembly, TTestCollection, TTestCase}.RunTestCollection"/>
    /// that properly obeys the parallel algorithm requirements.
    /// </summary>
    public new async ValueTask<RunSummary> RunTestCollection(IXunitTestCollection testCollection,
        IReadOnlyCollection<IXunitTestCase> testCases,
        ITestCaseOrderer testCaseOrderer) =>
        await new DependencyInjectionTestCollectionRunner(context,
            context.DefaultRootServices?.GetService<ITestClassOrderer>()).Run(
            testCollection,
            testCases,
            ExplicitOption,
            MessageBus,
            testCaseOrderer,
            Aggregator.Clone(),
            CancellationTokenSource,
            AssemblyFixtureMappings);
}
