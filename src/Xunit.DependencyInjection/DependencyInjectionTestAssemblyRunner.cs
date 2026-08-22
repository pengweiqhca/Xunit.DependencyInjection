namespace Xunit.DependencyInjection;

internal class DependencyInjectionTestAssemblyRunner(
    HostManager hostManager,
    IAsyncExceptionFilter? exceptionFilter,
    DependencyInjectionStartupContext context,
    IReadOnlyCollection<Exception> exceptions)
    : XunitTestAssemblyRunnerBase<DependencyInjectionAssemblyRunnerContext, DependencyInjectionTestAssembly,
        IXunitTestCollection,
        IXunitTestCase>
{
    protected override async ValueTask<bool> OnTestAssemblyStarting(DependencyInjectionAssemblyRunnerContext ctxt)
    {
        try
        {
            await hostManager.StartAsync(ctxt.CancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            ctxt.Aggregator.Add(exceptionFilter?.Process(ex) ?? ex);
        }

        if (exceptions.Count > 0)
        {
            foreach (var ex in exceptions)
                ctxt.Aggregator.Add(exceptionFilter?.Process(ex) ?? ex);
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

    protected override async ValueTask<bool> OnTestAssemblyFinished(DependencyInjectionAssemblyRunnerContext ctxt,
        RunSummary summary)
    {
        if (context.DefaultRootServices != null)
            ctxt.AssemblyFixtureMappings.ClearFixtures(ctxt.TestAssembly.AssemblyFixtureTypes,
                context.DefaultRootServices);

        try
        {
            await hostManager.StopAsync(ctxt.CancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            ctxt.Aggregator.Add(exceptionFilter?.Process(ex) ?? ex);
            summary.Failed = ctxt.TestCases.Count;
        }

        return await base.OnTestAssemblyFinished(ctxt, summary);
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

/// <inheritdoc />
protected override ValueTask<RunSummary> RunTestCollection(DependencyInjectionAssemblyRunnerContext ctxt,
    IXunitTestCollection testCollection,
    IReadOnlyCollection<IXunitTestCase> testCases) =>
    ctxt.RunTestCollection(testCollection, testCases);
}

/// <inheritdoc />
public class DependencyInjectionAssemblyRunnerContext(
    DependencyInjectionStartupContext context,
    DependencyInjectionTestAssembly testAssembly,
    IReadOnlyCollection<IXunitTestCase> testCases,
    IMessageSink executionMessageSink,
    ITestFrameworkExecutionOptions executionOptions,
    CancellationToken cancellationToken)
    : XunitTestAssemblyRunnerBaseContext<DependencyInjectionTestAssembly, IXunitTestCollection, IXunitTestCase>(
        testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken)
{
    public override async ValueTask<RunSummary> RunTestCollection(IXunitTestCollection testCollection,
        IReadOnlyCollection<IXunitTestCase> testCases) =>
        await new DependencyInjectionTestCollectionRunner(context).Run(
            testCollection,
            testCases,
            ExplicitOption,
            MessageBus,
            Aggregator.Clone(),
            CancellationTokenSource,
            ParallelMode,
            Scheduler,
            AssemblyFixtureMappings);
}
