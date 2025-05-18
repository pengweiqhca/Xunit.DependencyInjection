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
    /// <param name="cancellationToken">The cancellation token used to cancel execution</param>
    public async ValueTask<RunSummary> Run(DependencyInjectionTestAssembly testAssembly,
        IReadOnlyCollection<IXunitTestCase> testCases,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions,
        CancellationToken cancellationToken)
    {
        await using var ctxt = new DependencyInjectionAssemblyRunnerContext(context, testAssembly,
            testCases, executionMessageSink, executionOptions, cancellationToken);

        await ctxt.InitializeAsync();

        return await Run(ctxt);
    }

#pragma warning disable CA2012 // We guarantee that parallel ValueTasks are only awaited once

	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestCollections(
        DependencyInjectionAssemblyRunnerContext ctxt,
		Exception? exception)
	{
		if (ctxt.DisableParallelization || exception is not null)
			return await base.RunTestCollections(ctxt, exception);

		ctxt.SetupParallelism();

		Func<Func<ValueTask<RunSummary>>, ValueTask<RunSummary>> taskRunner;
		if (SynchronizationContext.Current is not null)
		{
			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			taskRunner = code => new(Task.Factory.StartNew(() => code().AsTask(), ctxt.CancellationTokenSource.Token, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap());
		}
		else
			taskRunner = code => new(Task.Run(() => code().AsTask(), ctxt.CancellationTokenSource.Token));

		List<ValueTask<RunSummary>>? parallel = null;
		List<Func<ValueTask<RunSummary>>>? nonParallel = null;
		var summaries = new List<RunSummary>();

        // If it has a custom TestCollectionOrderer, we need to run the collections in the order.
        var previous = ctxt.TestAssembly.TestCollectionOrderer is null or DefaultTestCollectionOrderer
            ? null
            : new SemaphoreSlim(1, 1);

		foreach (var (collection, testCases) in OrderTestCollections(ctxt))
		{
			ValueTask<RunSummary> Run() => RunTestCollection(ctxt, collection, testCases);
			if (collection.DisableParallelization)
				(nonParallel ??= []).Add(Run);
            else if (previous == null)
                (parallel ??= []).Add(taskRunner(Run));
			else
            {
                var current = previous;
                var next = new SemaphoreSlim(0, 1);
                previous = next;
                (parallel ??= []).Add(taskRunner(async () =>
                {
                    // Keep TestCollection order
                    await current.WaitAsync();
                    try
                    {
                        return await Run();
                    }
                    finally
                    {
                        next.Release();
                        current.Dispose();
                    }
                }));
            }
		}

		if (parallel?.Count > 0)
			foreach (var task in parallel)
				try
				{
					summaries.Add(await task);
				}
				catch (TaskCanceledException) { }

		if (nonParallel?.Count > 0)
			foreach (var taskFactory in nonParallel)
				try
				{
					summaries.Add(await taskRunner(taskFactory));
					if (ctxt.CancellationTokenSource.IsCancellationRequested)
						break;
				}
				catch (TaskCanceledException) { }

		return new RunSummary()
		{
			Total = summaries.Sum(s => s.Total),
			Failed = summaries.Sum(s => s.Failed),
			NotRun = summaries.Sum(s => s.NotRun),
			Skipped = summaries.Sum(s => s.Skipped),
		};
	}

#pragma warning restore CA2012

    /// <inheritdoc />
    protected override ValueTask<RunSummary> RunTestCollection(DependencyInjectionAssemblyRunnerContext ctxt,
        IXunitTestCollection testCollection,
        IReadOnlyCollection<IXunitTestCase> testCases) =>
        ctxt.RunTestCollection(testCollection, testCases,
            ctxt.AssemblyTestCaseOrderer ?? DefaultTestCaseOrderer.Instance);
}

/// <inheritdoc />
public class DependencyInjectionAssemblyRunnerContext(
    DependencyInjectionStartupContext context,
    DependencyInjectionTestAssembly testAssembly,
    IReadOnlyCollection<IXunitTestCase> testCases,
    IMessageSink executionMessageSink,
    ITestFrameworkExecutionOptions executionOptions,
    CancellationToken cancellationToken)
    : XunitTestAssemblyRunnerBaseContext<DependencyInjectionTestAssembly, IXunitTestCase>(testAssembly, testCases,
        executionMessageSink, executionOptions, cancellationToken)
{
    public override void SetupParallelism()
    {
        base.SetupParallelism();

        context.MaxParallelThreads = MaxParallelThreads;

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
