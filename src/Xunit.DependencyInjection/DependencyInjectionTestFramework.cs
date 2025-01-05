namespace Xunit.DependencyInjection;

public sealed class DependencyInjectionTestFramework : XunitTestFramework
{
    private readonly string? _configFileName;

    public DependencyInjectionTestFramework() { }

    public DependencyInjectionTestFramework(string? configFileName) : base(configFileName) =>
        _configFileName = configFileName;

    protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly) =>
        new DependencyInjectionTestFrameworkExecutor(
            new XunitTestAssembly(assembly, _configFileName, assembly.GetName().Version), ParallelizationMode.None);
}

public sealed class DependencyInjectionEnhancedParallelizationTestFramework
    : XunitTestFramework
{
    private readonly string? _configFileName;

    public DependencyInjectionEnhancedParallelizationTestFramework() { }

    public DependencyInjectionEnhancedParallelizationTestFramework(string? configFileName) : base(configFileName) =>
        _configFileName = configFileName;

    protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly) =>
        new DependencyInjectionTestFrameworkExecutor(
            new XunitTestAssembly(assembly, _configFileName, assembly.GetName().Version), ParallelizationMode.Enhance);
}

public sealed class DependencyInjectionForcedParallelizationTestFramework
    : XunitTestFramework
{
    private readonly string? _configFileName;

    public DependencyInjectionForcedParallelizationTestFramework() { }

    public DependencyInjectionForcedParallelizationTestFramework(string? configFileName) : base(configFileName) =>
        _configFileName = configFileName;

    protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly) =>
        new DependencyInjectionTestFrameworkExecutor(
            new XunitTestAssembly(assembly, _configFileName, assembly.GetName().Version), ParallelizationMode.Force);
}
