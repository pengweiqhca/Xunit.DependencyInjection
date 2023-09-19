namespace Xunit.DependencyInjection;

public class DependencyInjectionTestFrameworkExecutor : XunitTestFrameworkExecutor
{
    private readonly ParallelizationMode _parallelizationMode;
    private readonly HostManager _hostManager;

    public DependencyInjectionTestFrameworkExecutor(AssemblyName assemblyName,
        ISourceInformationProvider sourceInformationProvider,
        ParallelizationMode parallelizationMode,
        IMessageSink diagnosticMessageSink)
        : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
    {
        _parallelizationMode = parallelizationMode;

        DisposalTracker.Add(_hostManager = new(assemblyName, diagnosticMessageSink));
    }

    /// <inheritdoc />
    protected override async void RunTestCases(
        IEnumerable<IXunitTestCase> testCases,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions)
    {
        testCases = testCases.ToList();

        var exceptions = new List<Exception>();

        var host = GetHost(exceptions, _hostManager.BuildDefaultHost)?.Host;

        static DependencyInjectionContext? GetHost(ICollection<Exception> exceptions, Func<DependencyInjectionContext?> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex.Unwrap());
            }

            return default;
        }

        var contextMap = testCases
            .GroupBy(tc => tc.TestMethod.TestClass)
            .ToDictionary(group => group.Key, group => GetHost(exceptions, () => _hostManager.GetContext(group.Key.Class.ToRuntimeType())));

        try
        {
            await _hostManager.StartAsync(default);
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }

        using var runner = new DependencyInjectionTestAssemblyRunner(new(host, _parallelizationMode, contextMap), TestAssembly,
            testCases, DiagnosticMessageSink, executionMessageSink, executionOptions, exceptions);

        await runner.RunAsync();

        await _hostManager.StopAsync(default);
    }
}
