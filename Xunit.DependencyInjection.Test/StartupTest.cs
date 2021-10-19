using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection.Demystifier;
using Xunit.DependencyInjection.Logging;

namespace Xunit.DependencyInjection.Test
{
    public static class MyDiScope
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
                StartupThatWasUsed = this.GetType();

                hostBuilder.ConfigureAppConfiguration(lb => lb.AddJsonFile("appsettings.json", false, true))
                           .UseServiceProviderFactory(new AutofacServiceProviderFactory());
            }

            public void ConfigureServices(IServiceCollection services) =>
                services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Debug))
                        .AddScoped<IDependency, DependencyClass>()
                        .AddScoped<IDependencyWithManagedLifetime, DependencyWithManagedLifetime>()
                        .AddHostedService<HostServiceTest>()
                        .AddSkippableFactSupport()
                        .AddSingleton<IAsyncExceptionFilter, DemystifyExceptionFilter>()
                        .AddSingleton<Dependency>();

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
            public StartupTest(Dependency dependency) {
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

    public static class MyDiScope2
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
                StartupThatWasUsed = this.GetType();

                hostBuilder.ConfigureAppConfiguration(lb => lb.AddJsonFile("appsettings.json", false, true))
                           .UseServiceProviderFactory(new AutofacServiceProviderFactory());
            }

            public void ConfigureServices(IServiceCollection services) =>
                services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Debug))
                        .AddScoped<IDependency, DependencyClass>()
                        .AddScoped<IDependencyWithManagedLifetime, DependencyWithManagedLifetime>()
                        .AddHostedService<HostServiceTest>()
                        .AddSkippableFactSupport()
                        .AddSingleton<IAsyncExceptionFilter, DemystifyExceptionFilter>()
                        .AddSingleton<Dependency>();

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
            public StartupTest(Dependency dependency) {
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

    public class StartupTest
    {
        private static readonly AssemblyName Name = Assembly.GetExecutingAssembly().GetName();

        [Fact]
        public void GetStartupTypeTest()
        {
            Assert.Equal(typeof(Startup), StartupLoader.GetAssemblyStartupType(Name));

            Assert.Equal(typeof(Startup), StartupLoader.GetAssemblyStartupType(new AssemblyName("Xunit.DependencyInjection.FakeTest")));
        }

#region CreateStartupTest

        public class EmptyStartup { }

        public class CreateStartupTestStartup1
        {
            public CreateStartupTestStartup1() { }
        }

        public class CreateStartupTestStartup2
        {
            private CreateStartupTestStartup2() { }
        }

        public class CreateStartupTestStartup3
        {
            public CreateStartupTestStartup3(AssemblyName name) { }
        }

        public class CreateStartupTestStartup4
        {
            public CreateStartupTestStartup4() { }
            public CreateStartupTestStartup4(Assembly assembly) { }
        }

        [Fact]
        public void CreateStartupTest()
        {
            Assert.Null(StartupLoader.CreateStartup(null));

            Assert.NotNull(StartupLoader.CreateStartup(typeof(EmptyStartup)));

            Assert.NotNull(StartupLoader.CreateStartup(typeof(CreateStartupTestStartup1)));

            Assert.Throws<InvalidOperationException>(() => StartupLoader.CreateStartup(typeof(CreateStartupTestStartup2)));

            Assert.Throws<InvalidOperationException>(() => StartupLoader.CreateStartup(typeof(CreateStartupTestStartup3)));

            Assert.Throws<InvalidOperationException>(() => StartupLoader.CreateStartup(typeof(CreateStartupTestStartup4)));
        }

#endregion

#region CreateHostBuilderTest

        public class CreateHostBuilderTestStartup0
        {
            public void CreateHostBuilder() { }
        }

        public class CreateHostBuilderTestStartup1
        {
            public IHostBuilder CreateHostBuilder() => new HostBuilder().ConfigureServices(services => services.AddSingleton(this));
        }

        public class CreateHostBuilderTestStartup2
        {
            public HostBuilder CreateHostBuilder()
            {
                var hostBuilder = new HostBuilder();

                hostBuilder.ConfigureServices(services => services.AddSingleton(this));

                return hostBuilder;
            }
        }

        public class CreateHostBuilderTestStartup3
        {
            public IHostBuilder CreateHostBuilder(IHostBuilder builder) => builder.ConfigureServices(services => services.AddSingleton(this));
        }

        public class CreateHostBuilderTestStartup4
        {
            public IHostBuilder CreateHostBuilder(AssemblyName name) => new HostBuilder().ConfigureServices(services => services.AddSingleton(name));
        }

        public class CreateHostBuilderTestStartup5
        {
            public void CreateHostBuilder(IHostBuilder builder) { }
            public void CreateHostBuilder(StringBuilder builder) { }
        }

        [Fact]
        public void CreateHostBuilderTest()
        {
            Assert.Null(StartupLoader.CreateHostBuilder(new EmptyStartup(), Name));

            Assert.Throws<InvalidOperationException>(() => StartupLoader.CreateHostBuilder(new CreateHostBuilderTestStartup0(), new AssemblyName()));

            Assert.Throws<InvalidOperationException>(() => StartupLoader.CreateHostBuilder(new CreateHostBuilderTestStartup0(), Name));

            object startup = new CreateHostBuilderTestStartup1();
            Assert.Equal(startup, StartupLoader.CreateHostBuilder(startup, Name)?.Build().Services.GetService<CreateHostBuilderTestStartup1>());

            startup = new CreateHostBuilderTestStartup2();
            Assert.Equal(startup, StartupLoader.CreateHostBuilder(startup, Name)?.Build().Services.GetService<CreateHostBuilderTestStartup2>());

            Assert.Throws<InvalidOperationException>(() => StartupLoader.CreateHostBuilder(new CreateHostBuilderTestStartup3(), Name));

            Assert.Equal(Name, StartupLoader.CreateHostBuilder(new CreateHostBuilderTestStartup4(), Name)?.Build().Services.GetService<AssemblyName>());

            Assert.Throws<InvalidOperationException>(() => StartupLoader.CreateHostBuilder(new CreateHostBuilderTestStartup5(), Name));
        }

#endregion

#region ConfigureHostTest

        public class ConfigureHostTestStartup0
        {
            public void ConfigureHost() { }
        }

        public class ConfigureHostTestStartup1
        {
            public void ConfigureHost(IHostBuilder builder) => builder.ConfigureServices(services => services.AddSingleton(this));
        }

        public class ConfigureHostTestStartup2
        {
            public void ConfigureHost(StringBuilder builder) { }
        }

        public class ConfigureHostTestStartup3
        {
            public void ConfigureHost(IHostBuilder builder, AssemblyName name) { }
        }

        public class ConfigureHostTestStartup4
        {
            public IHostBuilder ConfigureHost(IHostBuilder builder) => builder.ConfigureServices(services => services.AddSingleton(this));
        }

        public class ConfigureHostTestStartup7
        {
            public void ConfigureHost(IHostBuilder builder) { }
            public void ConfigureHost(StringBuilder builder) { }
        }

        public class ConfigureHostTestStartup8
        {
            public static void ConfigureHost(IHostBuilder builder) { }
        }

        [Fact]
        public void ConfigureHostTest()
        {
            var hostBuilder = new HostBuilder();

            StartupLoader.ConfigureHost(hostBuilder, new EmptyStartup());

            Assert.Throws<InvalidOperationException>(() => StartupLoader.ConfigureHost(hostBuilder, new ConfigureHostTestStartup0()));

            StartupLoader.ConfigureHost(hostBuilder, new ConfigureHostTestStartup1());

            Assert.Throws<InvalidOperationException>(() => StartupLoader.ConfigureHost(hostBuilder, new ConfigureHostTestStartup2()));

            Assert.Throws<InvalidOperationException>(() => StartupLoader.ConfigureHost(hostBuilder, new ConfigureHostTestStartup3()));

            Assert.Throws<InvalidOperationException>(() => StartupLoader.ConfigureHost(hostBuilder, new ConfigureHostTestStartup4()));

            Assert.Throws<InvalidOperationException>(() => StartupLoader.ConfigureHost(hostBuilder, new ConfigureHostTestStartup7()));

            var services = hostBuilder.Build().Services;
            Assert.NotNull(services.GetService<ConfigureHostTestStartup1>());
        }

#endregion

#region ConfigureServicesTest

        public class ConfigureServicesTestStartup0
        {
            public void ConfigureServices() { }
        }

        public class ConfigureServicesTestStartup1
        {
            public void ConfigureServices(IServiceCollection services) => services.AddSingleton(this);
        }

        public class ConfigureServicesTestStartup2
        {
            public void ConfigureServices(StringBuilder builder) { }
        }

        public class ConfigureServicesTestStartup3
        {
            public void ConfigureServices(IServiceCollection services, HostBuilderContext context) => services.AddSingleton(this);
        }

        public class ConfigureServicesTestStartup4
        {
            public void ConfigureServices(HostBuilderContext context, IServiceCollection services) => services.AddSingleton(this);
        }

        public class ConfigureServicesTestStartup5
        {
            public void ConfigureServices(IServiceCollection services, object context) { }
        }

        public class ConfigureServicesTestStartup6
        {
            public void ConfigureServices(HostBuilderContext context, IServiceCollection services, object obj) { }
        }

        public class ConfigureServicesTestStartup7
        {
            public void ConfigureServices(IServiceCollection services, HostBuilderContext context) { }
            public void ConfigureServices(HostBuilderContext context, IServiceCollection services) { }
        }

        public class ConfigureServicesTestStartup8
        {
            public IServiceCollection ConfigureServices(IServiceCollection services) => services.AddSingleton(this);
        }

        [Fact]
        public void ConfigureServicesTest()
        {
            var hostBuilder = new HostBuilder();

            StartupLoader.ConfigureServices(hostBuilder, new EmptyStartup());

            Assert.Throws<InvalidOperationException>(() => StartupLoader.ConfigureServices(hostBuilder, new ConfigureServicesTestStartup0()));

            StartupLoader.ConfigureServices(hostBuilder, new ConfigureServicesTestStartup1());

            Assert.Throws<InvalidOperationException>(() => StartupLoader.ConfigureServices(hostBuilder, new ConfigureServicesTestStartup2()));

            StartupLoader.ConfigureServices(hostBuilder, new ConfigureServicesTestStartup3());

            StartupLoader.ConfigureServices(hostBuilder, new ConfigureServicesTestStartup4());

            Assert.Throws<InvalidOperationException>(() => StartupLoader.ConfigureServices(hostBuilder, new ConfigureServicesTestStartup5()));

            Assert.Throws<InvalidOperationException>(() => StartupLoader.ConfigureServices(hostBuilder, new ConfigureServicesTestStartup6()));

            Assert.Throws<InvalidOperationException>(() => StartupLoader.ConfigureServices(hostBuilder, new ConfigureServicesTestStartup7()));

            Assert.Throws<InvalidOperationException>(() => StartupLoader.ConfigureServices(hostBuilder, new ConfigureServicesTestStartup8()));

            var services = hostBuilder.Build().Services;
            Assert.NotNull(services.GetService<ConfigureServicesTestStartup1>());
            Assert.NotNull(services.GetService<ConfigureServicesTestStartup3>());
            Assert.NotNull(services.GetService<ConfigureServicesTestStartup4>());
        }

#endregion

#region ConfigureTest

        public class ConfigureTestStartup0
        {
            public IServiceCollection Configure(IServiceCollection services, HostBuilderContext context) => services;
        }

        public class ConfigureTestStartup1
        {
            public void Configure(IServiceCollection services, HostBuilderContext context) { }
            public void Configure(HostBuilderContext context, IServiceCollection services) { }
        }

        public class ConfigureTestStartup2
        {
            public bool Invoked { get; set; }

            public void Configure(IServiceCollection? services, ConfigureTestStartup2 self)
            {
                Assert.Null(services);

                Assert.Equal(this, self);

                Invoked = true;
            }

            public override string ToString() => GetHashCode().ToString();
        }

        [Fact]
        public void ConfigureTest()
        {
            var startup = new ConfigureTestStartup2();
            var hostBuilder = new HostBuilder().ConfigureServices(s => s.AddSingleton(startup));

            var services = hostBuilder.Build().Services;

            StartupLoader.Configure(services, new EmptyStartup());

            Assert.Throws<InvalidOperationException>(() => StartupLoader.Configure(services, new ConfigureTestStartup0()));

            Assert.Throws<InvalidOperationException>(() => StartupLoader.Configure(services, new ConfigureTestStartup1()));

            StartupLoader.Configure(services, startup);

            Assert.True(startup.Invoked);
        }

#endregion
    }
}
