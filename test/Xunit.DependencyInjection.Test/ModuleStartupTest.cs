namespace Xunit.DependencyInjection.Test;

public static class ModuleStartupTest
{
    private static Type? StartupThatWasUsed { get; set; }

    public class Dependency
    {
        public string Value => "Wow2";
    }

    public class Startup
    {
        public void ConfigureHost(IHostBuilder hostBuilder)
        {
            StartupThatWasUsed = GetType();
            hostBuilder.ConfigureAppConfiguration(lb => lb.AddJsonFile("appsettings.json", false, true));
        }

        public void ConfigureServices(IServiceCollection services) =>
            services.AddSingleton<Dependency>();

        public void Configure(IServiceProvider provider, ITestOutputHelperAccessor accessor)
        {
            Assert.NotNull(accessor);
            XunitTestOutputLoggerProvider.Register(provider);
        }
    }

    public class StartupTest
    {
        public Dependency Dependency { get; }

        public StartupTest(Dependency dependency) => Dependency = dependency;

        [Fact]
        public void ProperStartupWasUsed() => Assert.Equal(typeof(Startup), StartupThatWasUsed);

        [Fact]
        public void DependencyIsInjectedInInnerScope() => Assert.Equal("Wow2", Dependency.Value);
    }
}
