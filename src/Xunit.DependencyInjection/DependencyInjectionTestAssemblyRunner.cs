namespace Xunit.DependencyInjection;

public class DependencyInjectionTestAssemblyRunner : XunitTestAssemblyRunner
{
    private readonly DependencyInjectionStartupContext _context;

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
    }

    protected override string GetTestFrameworkEnvironment() => base.GetTestFrameworkEnvironment();

    /// <inheritdoc />
    protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus,
        ITestCollection testCollection,
        IEnumerable<IXunitTestCase> testCases,
        CancellationTokenSource cancellationTokenSource) =>
        new DependencyInjectionTestCollectionRunner(_context, testCollection, testCases, DiagnosticMessageSink,
            messageBus, TestCaseOrderer, new(Aggregator), cancellationTokenSource).RunAsync();
}
