using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.DependencyInjection.Logging;

namespace Xunit.DependencyInjection.Test
{
    [Startup(typeof(Startup2))]
    public class StartupAttributeTest
    {
        private static Type? StartupThatWasUsed { get; set; }

        public Dependency2 Dependency { get; }

        public StartupAttributeTest(Dependency2 dependency) => Dependency = dependency;

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
                XunitTestOutputLoggerProvider.Register(provider);
            }
        }
    }

    [Startup(typeof(StartupAttributeTest.Startup2), Shared = false)]
    public class StartupAttributeSharedTest
    {
        [Fact]
        public void SharedTest() => Assert.Equal(2, StartupAttributeTest.Startup2.Counter);
    }
}
