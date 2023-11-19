using FluentAssertions;

namespace Xunit.DependencyInjection.Test;

[Startup(typeof(Startup2))]
public class StartupAttributeTest(StartupAttributeTest.Dependency2 dependency)
{
    private static Type? StartupThatWasUsed { get; set; }

    public Dependency2 Dependency { get; } = dependency;

    [Fact]
    public void ProperStartupWasUsed() => Assert.Equal(typeof(Startup2), StartupThatWasUsed);

    [Fact]
    public void DependencyIsInjectedInInnerScope() => Assert.Equal("Wow", Dependency.Value);

    public class Dependency2
    {
        public string Value => "Wow";
    }

    public class Startup2
    {
        public static int Counter { get; set; }

        public Startup2() => Counter++;

        public void ConfigureHost(IHostBuilder hostBuilder)
        {
            StartupThatWasUsed = GetType();
            hostBuilder.ConfigureAppConfiguration(lb => lb.AddJsonFile("appsettings.json", false, true));
        }

        public void ConfigureServices(IServiceCollection services) =>
            services.AddSingleton<Dependency2>();

        public void Configure(IServiceProvider provider, ITestOutputHelperAccessor accessor)
        {
            Assert.NotNull(accessor);
 #pragma warning disable CS0618 // Type or member is obsolete
            XunitTestOutputLoggerProvider.Register(provider);
 #pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}

[Startup(typeof(StartupAttributeTest.Startup2), Shared = false)]
public class StartupAttributeSharedTest
{
    [Fact]
    public void SharedTest() => StartupAttributeTest.Startup2.Counter.Should().BeGreaterOrEqualTo(1);
}
