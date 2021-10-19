using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.DependencyInjection.Logging;

namespace Xunit.DependencyInjection.Test
{
    public static class DependencyInjectionScope
    {
        private static Type? StartupThatWasUsed { get; set; }

        public class Dependency
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

            private static readonly AssemblyName Name = Assembly.GetExecutingAssembly().GetName();

            public StartupTest(Dependency dependency)
            {
                Dependency = dependency;
            }

            [Fact]
            public void GetStartupTypeTest()
            {
                var startupType = StartupLoader.GetAssemblyStartupType(Name);
                var moduleStartupTypes = StartupLoader.GetModuleStartupTypes(startupType);

                Assert.Equal(typeof(Startup), moduleStartupTypes[0].StartupType);
            }

            [Fact]
            public void ProperStartupWasUsed()
            {
                Assert.Equal(typeof(Startup), StartupThatWasUsed);
            }

            [Fact]
            public void DependencyIsInjectedInInnerScope()
            {
                Assert.Equal("Wow", Dependency.Value);
            }
        }
    }

    public static class DependencyInjectionScope2
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

            private static readonly AssemblyName Name = Assembly.GetExecutingAssembly().GetName();

            public StartupTest(Dependency dependency)
            {
                Dependency = dependency;
            }

            [Fact]
            public void GetStartupTypeTest()
            {
                var startupType = StartupLoader.GetAssemblyStartupType(Name);
                var moduleStartupTypes = StartupLoader.GetModuleStartupTypes(startupType);

                Assert.Equal(typeof(Startup), moduleStartupTypes[1].StartupType);
            }

            [Fact]
            public void ProperStartupWasUsed()
            {
                Assert.Equal(typeof(Startup), StartupThatWasUsed);
            }

            [Fact]
            public void DependencyIsInjectedInInnerScope()
            {
                Assert.Equal("Wow2", Dependency.Value);
            }
        }
    }
}
