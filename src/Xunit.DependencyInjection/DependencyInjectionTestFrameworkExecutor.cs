namespace Xunit.DependencyInjection;

public class DependencyInjectionTestFrameworkExecutor(
    AssemblyName assemblyName,
    ISourceInformationProvider sourceInformationProvider,
    ParallelizationMode parallelizationMode,
    IMessageSink diagnosticMessageSink)
    : XunitTestFrameworkExecutor(assemblyName, sourceInformationProvider, diagnosticMessageSink)
{
    /// <inheritdoc />
    protected override void RunTestCases(IEnumerable<IXunitTestCase> testCases,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions) =>
        RunTestCasesAsync(testCases, executionMessageSink, executionOptions).GetAwaiter().GetResult();

    private async Task RunTestCasesAsync(
        IEnumerable<IXunitTestCase> testCases,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions)
    {
        testCases = [.. testCases];

        var exceptions = new List<Exception>();

        await using var hostManager = new HostManager(((ReflectionAssemblyInfo)AssemblyInfo).Assembly, DiagnosticMessageSink);

        var host = GetHost(exceptions, hostManager.BuildDefaultHost)?.Host;

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
            .ToDictionary(group => group.Key, group => GetHost(exceptions, () => hostManager.GetContext(group.Key.Class.ToRuntimeType())));

        try
        {
            await hostManager.StartAsync(default);
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }

        using var runner = new DependencyInjectionTestAssemblyRunner(new(host, parallelizationMode, contextMap), TestAssembly,
            testCases, DiagnosticMessageSink, executionMessageSink, executionOptions, exceptions);

        await runner.RunAsync();

        await hostManager.StopAsync(default);
    }
}
