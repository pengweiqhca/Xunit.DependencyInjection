namespace Xunit.DependencyInjection;

public sealed class DependencyInjectionTestFramework : XunitTestFramework
{
    public DependencyInjectionTestFramework(IMessageSink messageSink) : base(messageSink) { }

    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName) =>
        new DependencyInjectionTestFrameworkExecutor(assemblyName, SourceInformationProvider, ParallelizationMode.None,
            DiagnosticMessageSink);
}

public sealed class DependencyInjectionEnhancedParallelizationTestFramework : XunitTestFramework
{
    public DependencyInjectionEnhancedParallelizationTestFramework(IMessageSink messageSink) : base(messageSink) { }

    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName) =>
        new DependencyInjectionTestFrameworkExecutor(assemblyName, SourceInformationProvider, ParallelizationMode.Enhance, DiagnosticMessageSink);
}

public sealed class DependencyInjectionForcedParallelizationTestFramework : XunitTestFramework
{
    public DependencyInjectionForcedParallelizationTestFramework(IMessageSink messageSink) : base(messageSink) { }

    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName) =>
        new DependencyInjectionTestFrameworkExecutor(assemblyName, SourceInformationProvider, ParallelizationMode.Force, DiagnosticMessageSink);
}
