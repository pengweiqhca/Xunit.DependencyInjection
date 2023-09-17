namespace Xunit.DependencyInjection;

public class DependencyInjectionContext
{
    public IHost Host { get; }

    public IServiceProvider RootServices => Host.Services;

    public bool DisableParallelization { get; }

    public DependencyInjectionContext(IHost host, bool disableParallelization)
    {
        Host = host;
        DisableParallelization = disableParallelization;
    }
}

public class DependencyInjectionStartupContext
{
    private readonly IHost? _defaultHost;

    public IServiceProvider? DefaultRootServices => _defaultHost?.Services;

    public ParallelizationMode ParallelizationMode { get; }

    public IReadOnlyDictionary<ITestClass, DependencyInjectionContext?> ContextMap { get; }

    public DependencyInjectionStartupContext(IHost? defaultHost, ParallelizationMode parallelizationMode,
        IReadOnlyDictionary<ITestClass, DependencyInjectionContext?> contextMap)
    {
        _defaultHost = defaultHost;
        ParallelizationMode = parallelizationMode;
        ContextMap = contextMap;
    }
}

public enum ParallelizationMode
{
    None = 0,
    Enhance = 1,
    Force = 2
}
