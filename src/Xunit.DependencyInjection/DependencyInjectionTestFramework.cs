namespace Xunit.DependencyInjection;

public sealed class DependencyInjectionTestFramework : XunitTestFramework
{
    private readonly string? _configFileName;

    public DependencyInjectionTestFramework() { }

    public DependencyInjectionTestFramework(string? configFileName) : base(configFileName) =>
        _configFileName = configFileName;

    protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly) =>
        new DependencyInjectionTestFrameworkExecutor(
            new XunitTestAssembly(assembly, _configFileName, version: assembly.GetName().Version));
}
