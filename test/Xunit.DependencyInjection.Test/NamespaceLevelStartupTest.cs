// ReSharper disable once CheckNamespace
namespace Xunit.DependencyInjection.Test.Test;

public class Dependency
{
    public string Value => "Wow2";
}

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder)
    {
        NamespaceLevelStartupTest.StartupThatWasUsed = GetType();
        hostBuilder.ConfigureAppConfiguration(lb => lb.AddJsonFile("appsettings.json", false, true));
    }

    public void ConfigureServices(IServiceCollection services) =>
        services.AddSingleton<ModuleStartupTest.Dependency>();
}

public class NamespaceLevelStartupTest(ModuleStartupTest.Dependency dependency)
{
    public static Type? StartupThatWasUsed { get; set; }

    public ModuleStartupTest.Dependency Dependency { get; } = dependency;

    [Fact]
    public void ProperStartupWasUsed() => Assert.Equal(typeof(Startup), StartupThatWasUsed);

    [Fact]
    public void DependencyIsInjectedInInnerScope() => Assert.Equal("Wow2", Dependency.Value);
}
