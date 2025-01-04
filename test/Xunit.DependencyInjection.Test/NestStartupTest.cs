using System.Diagnostics;

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
        public static List<Activity> Activities { get; } = [];

        public void ConfigureHost(IHostBuilder hostBuilder)
        {
            StartupThatWasUsed = GetType();
            hostBuilder.ConfigureAppConfiguration(lb => lb.AddJsonFile("appsettings.json", false, true));
        }

        public void ConfigureServices(IServiceCollection services) =>
            services.AddSingleton<Dependency2>();

        public void Configure(ITestOutputHelperAccessor accessor)
        {
            Assert.NotNull(accessor);

            Activities.Clear();;

            var listener = new ActivityListener();

            listener.ShouldListenTo += _ => true;
            listener.ActivityStopped += Activities.Add;
            listener.Sample += delegate { return ActivitySamplingResult.AllDataAndRecorded; };

            ActivitySource.AddActivityListener(listener);
        }
    }
}

public class MonitorNestStartupTest
{
    [Fact]
    public void ActivityTest() => Assert.NotEmpty(NestStartupTest.Startup.Activities);
}
