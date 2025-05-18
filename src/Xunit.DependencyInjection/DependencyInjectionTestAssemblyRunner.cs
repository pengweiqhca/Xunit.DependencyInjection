﻿namespace Xunit.DependencyInjection;

public class DependencyInjectionTestAssemblyRunner : XunitTestAssemblyRunner
{
    private readonly DependencyInjectionStartupContext _context;
    private readonly ITestClassOrderer? _testClassOrderer;

    public DependencyInjectionTestAssemblyRunner(DependencyInjectionStartupContext context,
        ITestAssembly testAssembly,
        IEnumerable<IXunitTestCase> testCases,
        IMessageSink diagnosticMessageSink,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions,
        IEnumerable<Exception> exceptions)
        : base(testAssembly, testCases, diagnosticMessageSink,
            executionMessageSink, executionOptions)
    {
        _context = context;

        foreach (var exception in exceptions) Aggregator.Add(exception);

        var testCollectionOrderer = context.DefaultRootServices?.GetService<ITestCollectionOrderer>();
        if (testCollectionOrderer != null) TestCollectionOrderer = testCollectionOrderer;

        _testClassOrderer = context.DefaultRootServices?.GetService<ITestClassOrderer>();

        var testCaseOrderer = context.DefaultRootServices?.GetService<ITestCaseOrderer>();
        if (testCaseOrderer != null) TestCaseOrderer = testCaseOrderer;
    }

    /// <inheritdoc />
    protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus,
        ITestCollection testCollection,
        IEnumerable<IXunitTestCase> testCases,
        CancellationTokenSource cancellationTokenSource) =>
        new DependencyInjectionTestCollectionRunner(_context, testCollection, testCases, DiagnosticMessageSink,
            messageBus, TestCaseOrderer, _testClassOrderer, new(Aggregator), cancellationTokenSource).RunAsync();

    /// <inheritdoc/>
    protected override async Task<RunSummary> RunTestCollectionsAsync(IMessageBus messageBus,
        CancellationTokenSource cancellationTokenSource)
    {
        var type = typeof(XunitTestAssemblyRunner);

        type.GetField("originalSyncContext", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(this, SynchronizationContext.Current);

        if (type.GetField("disableParallelization", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(this) is true)
            return await base.RunTestCollectionsAsync(messageBus, cancellationTokenSource);

        _context.MaxParallelThreads =
            (int)type.GetField("maxParallelThreads", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(this);

        if (_context.MaxParallelThreads > 0)
        {
            var parallelAlgorithm = type.GetField("parallelAlgorithm", BindingFlags.Instance | BindingFlags.NonPublic);
            if (parallelAlgorithm != null && (int)parallelAlgorithm.GetValue(this) == 0)
            {
                _context.ParallelSemaphore = new(_context.MaxParallelThreads);

                type.GetField("parallelSemaphore", BindingFlags.Instance | BindingFlags.NonPublic)?
                    .SetValue(this, _context.ParallelSemaphore);

                ThreadPool.GetMinThreads(out var minThreads, out var minIOPorts);
                if (minThreads < _context.MaxParallelThreads)
                    ThreadPool.SetMinThreads(_context.MaxParallelThreads, minIOPorts);
            }
            else
                SetupSyncContext(_context.MaxParallelThreads);
        }

        Func<Func<Task<RunSummary>>, Task<RunSummary>> taskRunner = SynchronizationContext.Current != null
            ? code => Task.Factory.StartNew(code, cancellationTokenSource.Token,
                TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler,
                TaskScheduler.FromCurrentSynchronizationContext()).Unwrap()
            : code => Task.Run(code, cancellationTokenSource.Token);

        List<Task<RunSummary>>? parallel = null;
        List<Func<Task<RunSummary>>>? nonParallel = null;
        var summaries = new List<RunSummary>();

        // If it has a custom TestCollectionOrderer, we need to run the collections in the order.
        var previous = TestCollectionOrderer is DefaultTestCollectionOrderer ? null : new SemaphoreSlim(1, 1);

        foreach (var collection in OrderTestCollections())
        {
            Task<RunSummary> Run() =>
                RunTestCollectionAsync(messageBus, collection.Item1, collection.Item2, cancellationTokenSource);

            // attr is null here from our new unit test, but I'm not sure if that's expected or there's a cheaper approach here
            // Current approach is trying to avoid any changes to the abstractions at all
            var attr = collection.Item1.CollectionDefinition?.GetCustomAttributes(typeof(CollectionDefinitionAttribute))
                .SingleOrDefault();

            if (attr?.GetNamedArgument<bool>(nameof(CollectionDefinitionAttribute.DisableParallelization)) == true)
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
            foreach (var task in nonParallel)
                try
                {
                    summaries.Add(await taskRunner(task));
                    if (cancellationTokenSource.IsCancellationRequested)
                        break;
                }
                catch (TaskCanceledException) { }

        return new()
        {
            Total = summaries.Sum(s => s.Total),
            Failed = summaries.Sum(s => s.Failed),
            Skipped = summaries.Sum(s => s.Skipped)
        };
    }
}
