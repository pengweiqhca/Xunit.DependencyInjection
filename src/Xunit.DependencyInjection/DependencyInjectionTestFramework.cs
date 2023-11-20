namespace Xunit.DependencyInjection;

public sealed class DependencyInjectionTestFramework(IMessageSink messageSink) : XunitTestFramework(messageSink)
{
    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName) =>
        new DependencyInjectionTestFrameworkExecutor(assemblyName, SourceInformationProvider, ParallelizationMode.None,
            DiagnosticMessageSink);
}

public sealed class DependencyInjectionEnhancedParallelizationTestFramework(IMessageSink messageSink)
    : XunitTestFramework(messageSink)
{
    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName) =>
        new DependencyInjectionTestFrameworkExecutor(assemblyName, SourceInformationProvider, ParallelizationMode.Enhance, DiagnosticMessageSink);
}

public sealed class DependencyInjectionForcedParallelizationTestFramework(IMessageSink messageSink)
    : XunitTestFramework(messageSink)
{
    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName) =>
        new DependencyInjectionTestFrameworkExecutor(assemblyName, SourceInformationProvider, ParallelizationMode.Force, DiagnosticMessageSink);
}
