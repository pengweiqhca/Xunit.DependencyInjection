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

    public void Configure(IServiceProvider provider, ITestOutputHelperAccessor accessor)
    {
        Assert.NotNull(accessor);
 #pragma warning disable CS0618 // Type or member is obsolete
        XunitTestOutputLoggerProvider.Register(provider);
 #pragma warning restore CS0618 // Type or member is obsolete
    }
}

public class NamespaceLevelStartupTest
{
    public static Type? StartupThatWasUsed { get; set; }

    public ModuleStartupTest.Dependency Dependency { get; }

    public NamespaceLevelStartupTest(ModuleStartupTest.Dependency dependency) => Dependency = dependency;

    [Fact]
    public void ProperStartupWasUsed() => Assert.Equal(typeof(Startup), StartupThatWasUsed);

    [Fact]
    public void DependencyIsInjectedInInnerScope() => Assert.Equal("Wow2", Dependency.Value);
}
