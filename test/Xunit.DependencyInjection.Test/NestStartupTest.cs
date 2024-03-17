namespace Xunit.DependencyInjection.Test;

public class NestStartupTest(NestStartupTest.Dependency2 dependency)
{
    private static Type? StartupThatWasUsed { get; set; }

    public Dependency2 Dependency { get; } = dependency;

    [Fact]
    public void ProperStartupWasUsed() => Assert.Equal(typeof(Startup), StartupThatWasUsed);

    [Fact]
    public void DependencyIsInjectedInInnerScope() => Assert.Equal("Wow", Dependency.Value);

    public class Dependency2
    {
        public string Value => "Wow";
    }

    public class Startup
    {
        public void ConfigureHost(IHostBuilder hostBuilder)
        {
            StartupThatWasUsed = GetType();
            hostBuilder.ConfigureAppConfiguration(lb => lb.AddJsonFile("appsettings.json", false, true));
        }

        public void ConfigureServices(IServiceCollection services) =>
            services.AddSingleton<Dependency2>();

        public void Configure(ITestOutputHelperAccessor accessor) => Assert.NotNull(accessor);
    }
}
