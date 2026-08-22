namespace Xunit.DependencyInjection;

public class DependencyInjectionContext(IHost host, bool disableParallelization)
{
    public IHost Host { get; } = host;

    public IServiceProvider RootServices => Host.Services;

    public bool DisableParallelization { get; } = disableParallelization;
}

public class DependencyInjectionBuildContext(IHost host, bool disableParallelization) : DependencyInjectionContext(host, disableParallelization)
{
    public bool Disposed { get; set; }
}

public class DependencyInjectionStartupContext(
    IHost? defaultHost,
    IReadOnlyDictionary<IXunitTestClass, DependencyInjectionBuildContext?> contextMap)
{
    public IServiceProvider? DefaultRootServices => defaultHost?.Services;

    public IReadOnlyDictionary<IXunitTestClass, DependencyInjectionBuildContext?> ContextMap { get; } = contextMap;
}
