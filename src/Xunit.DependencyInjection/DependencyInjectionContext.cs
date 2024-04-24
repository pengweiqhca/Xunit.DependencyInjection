namespace Xunit.DependencyInjection;

public class DependencyInjectionContext(IHost host, bool disableParallelization)
{
    public IHost Host { get; } = host;

    public IServiceProvider RootServices => Host.Services;

    public bool DisableParallelization { get; } = disableParallelization;
}

public class DependencyInjectionTestContext(IHost host, bool disableParallelization, bool force) : DependencyInjectionContext(host, disableParallelization)
{
    public bool ForcedParallelization { get; } = force;
}

public class DependencyInjectionStartupContext(
    IHost? defaultHost,
    ParallelizationMode parallelizationMode,
    IReadOnlyDictionary<ITestClass, DependencyInjectionContext?> contextMap)
{
    public IServiceProvider? DefaultRootServices => defaultHost?.Services;

    public ParallelizationMode ParallelizationMode { get; } = parallelizationMode;

    public IReadOnlyDictionary<ITestClass, DependencyInjectionContext?> ContextMap { get; } = contextMap;
}

public enum ParallelizationMode
{
    None = 0,
    Enhance = 1,
    Force = 2
}
