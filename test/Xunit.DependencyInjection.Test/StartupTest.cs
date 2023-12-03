using System.Reflection;
using System.Text;

namespace Xunit.DependencyInjection.Test;

public class StartupTest(IMessageSink diagnosticMessageSink)
{
    private static readonly AssemblyName Name = Assembly.GetExecutingAssembly().GetName();

    [Fact]
    public void StartupSharedTest() => Assert.Equal(1, Startup.Counter);

    [Fact]
    public void GetStartupTypeTest()
    {
        Assert.Equal(typeof(Startup), StartupLoader.GetStartupType(Name));

        Assert.Equal(typeof(Startup), StartupLoader.GetStartupType(new("Xunit.DependencyInjection.FakeTest")));
    }

    #region CreateStartupTest
    public class EmptyStartup;
    public class CreateStartupTestStartup1;
    public class CreateStartupTestStartup2 { private CreateStartupTestStartup2() { } }
    public class CreateStartupTestStartup3 { public CreateStartupTestStartup3(AssemblyName name) { } }

    public class CreateStartupTestStartup4
    {
        public CreateStartupTestStartup4() { }
        public CreateStartupTestStartup4(Assembly assembly) { }
    }

    [Fact]
    public void CreateStartupTest()
    {
        Assert.Throws<ArgumentNullException>(() => StartupLoader.CreateStartup(null!));

        Assert.NotNull(StartupLoader.CreateStartup(typeof(EmptyStartup)));

        Assert.NotNull(StartupLoader.CreateStartup(typeof(CreateStartupTestStartup1)));

        Assert.Throws<InvalidOperationException>(() => StartupLoader.CreateStartup(typeof(CreateStartupTestStartup2)));

        Assert.Throws<InvalidOperationException>(() => StartupLoader.CreateStartup(typeof(CreateStartupTestStartup3)));

        Assert.Throws<InvalidOperationException>(() => StartupLoader.CreateStartup(typeof(CreateStartupTestStartup4)));
    }
    #endregion

    #region CreateHostBuilderTest

    public class CreateHostBuilderTestStartup0 { public void CreateHostBuilder() { } }
    public class CreateHostBuilderTestStartup1 { public IHostBuilder CreateHostBuilder() => new HostBuilder().ConfigureServices(services => services.AddSingleton(this)); }
    public class CreateHostBuilderTestStartup2
    {
        public HostBuilder CreateHostBuilder()
        {
            var hostBuilder = new HostBuilder();

            hostBuilder.ConfigureServices(services => services.AddSingleton(this));

            return hostBuilder;
        }
    }
    public class CreateHostBuilderTestStartup3 { public IHostBuilder CreateHostBuilder(IHostBuilder builder) => builder.ConfigureServices(services => services.AddSingleton(this)); }
    public class CreateHostBuilderTestStartup4 { public IHostBuilder CreateHostBuilder(AssemblyName name) => new HostBuilder().ConfigureServices(services => services.AddSingleton(name)); }
    public class CreateHostBuilderTestStartup5
    {
        public void CreateHostBuilder(IHostBuilder builder) { }
        public void CreateHostBuilder(StringBuilder builder) { }
    }

    [Fact]
    public void CreateHostBuilderTest()
    {
        static IHostBuilder? CreateHostBuilder(AssemblyName name, object startup) =>
            StartupLoader.CreateHostBuilder(name, startup, startup.GetType(),
                StartupLoader.FindMethod(startup.GetType(), nameof(CreateHostBuilder),
                    typeof(IHostBuilder)));

        Assert.Null(CreateHostBuilder(Name, new EmptyStartup()));

        Assert.Throws<InvalidOperationException>(() => CreateHostBuilder(new(), new CreateHostBuilderTestStartup0()));

        Assert.Throws<InvalidOperationException>(() => CreateHostBuilder(Name, new CreateHostBuilderTestStartup0()));

        object startup = new CreateHostBuilderTestStartup1();
        Assert.Equal(startup, CreateHostBuilder(Name, startup)?.Build().Services.GetService<CreateHostBuilderTestStartup1>());

        startup = new CreateHostBuilderTestStartup2();
        Assert.Equal(startup, CreateHostBuilder(Name, startup)?.Build().Services.GetService<CreateHostBuilderTestStartup2>());

        Assert.Throws<InvalidOperationException>(() => CreateHostBuilder(Name, new CreateHostBuilderTestStartup3()));

        Assert.Equal(Name, CreateHostBuilder(Name, new CreateHostBuilderTestStartup4())?.Build().Services.GetService<AssemblyName>());

        Assert.Throws<InvalidOperationException>(() => CreateHostBuilder(Name, new CreateHostBuilderTestStartup5()));
    }
    #endregion

    #region ConfigureHostTest

    public class ConfigureHostTestStartup0 { public void ConfigureHost() { } }
    public class ConfigureHostTestStartup1 { public void ConfigureHost(IHostBuilder builder) => builder.ConfigureServices(services => services.AddSingleton(this)); }
    public class ConfigureHostTestStartup2 { public void ConfigureHost(StringBuilder builder) { } }
    public class ConfigureHostTestStartup3 { public void ConfigureHost(IHostBuilder builder, AssemblyName name) { } }
    public class ConfigureHostTestStartup4 { public IHostBuilder ConfigureHost(IHostBuilder builder) => builder.ConfigureServices(services => services.AddSingleton(this)); }
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
        static void ConfigureHost(IHostBuilder hostBuilder, object startup) =>
            StartupLoader.ConfigureHost(hostBuilder, startup, startup.GetType(),
                StartupLoader.FindMethod(startup.GetType(), nameof(ConfigureHost)));

        var hostBuilder = new HostBuilder();

        ConfigureHost(hostBuilder, new EmptyStartup());

        Assert.Throws<InvalidOperationException>(() => ConfigureHost(hostBuilder, new ConfigureHostTestStartup0()));

        ConfigureHost(hostBuilder, new ConfigureHostTestStartup1());

        Assert.Throws<InvalidOperationException>(() => ConfigureHost(hostBuilder, new ConfigureHostTestStartup2()));

        Assert.Throws<InvalidOperationException>(() => ConfigureHost(hostBuilder, new ConfigureHostTestStartup3()));

        Assert.Throws<InvalidOperationException>(() => ConfigureHost(hostBuilder, new ConfigureHostTestStartup4()));

        Assert.Throws<InvalidOperationException>(() => ConfigureHost(hostBuilder, new ConfigureHostTestStartup7()));

        var services = hostBuilder.Build().Services;
        Assert.NotNull(services.GetService<ConfigureHostTestStartup1>());
    }
    #endregion

    #region ConfigureServicesTest

    public class ConfigureServicesTestStartup0 { public void ConfigureServices() { } }
    public class ConfigureServicesTestStartup1 { public void ConfigureServices(IServiceCollection services) => services.AddSingleton(this); }
    public class ConfigureServicesTestStartup2 { public void ConfigureServices(StringBuilder builder) { } }
    public class ConfigureServicesTestStartup3 { public void ConfigureServices(IServiceCollection services, HostBuilderContext context) => services.AddSingleton(this); }
    public class ConfigureServicesTestStartup4 { public void ConfigureServices(HostBuilderContext context, IServiceCollection services) => services.AddSingleton(this); }
    public class ConfigureServicesTestStartup5 { public void ConfigureServices(IServiceCollection services, object context) { } }
    public class ConfigureServicesTestStartup6 { public void ConfigureServices(HostBuilderContext context, IServiceCollection services, object obj) { } }
    public class ConfigureServicesTestStartup7
    {
        public void ConfigureServices(IServiceCollection services, HostBuilderContext context) { }
        public void ConfigureServices(HostBuilderContext context, IServiceCollection services) { }
    }
    public class ConfigureServicesTestStartup8 { public IServiceCollection ConfigureServices(IServiceCollection services) => services.AddSingleton(this); }

    [Fact]
    public void ConfigureServicesTest()
    {
        static void ConfigureServices(IHostBuilder hostBuilder, object startup) =>
            StartupLoader.ConfigureServices(hostBuilder, startup, startup.GetType(),
                StartupLoader.FindMethod(startup.GetType(), nameof(ConfigureServices)));

        var hostBuilder = new HostBuilder();

        ConfigureServices(hostBuilder, new EmptyStartup());

        Assert.Throws<InvalidOperationException>(() => ConfigureServices(hostBuilder, new ConfigureServicesTestStartup0()));

        ConfigureServices(hostBuilder, new ConfigureServicesTestStartup1());

        Assert.Throws<InvalidOperationException>(() => ConfigureServices(hostBuilder, new ConfigureServicesTestStartup2()));

        ConfigureServices(hostBuilder, new ConfigureServicesTestStartup3());

        ConfigureServices(hostBuilder, new ConfigureServicesTestStartup4());

        Assert.Throws<InvalidOperationException>(() => ConfigureServices(hostBuilder, new ConfigureServicesTestStartup5()));

        Assert.Throws<InvalidOperationException>(() => ConfigureServices(hostBuilder, new ConfigureServicesTestStartup6()));

        Assert.Throws<InvalidOperationException>(() => ConfigureServices(hostBuilder, new ConfigureServicesTestStartup7()));

        Assert.Throws<InvalidOperationException>(() => ConfigureServices(hostBuilder, new ConfigureServicesTestStartup8()));

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

        public void Configure(ConfigureTestStartup2 self)
        {
            Assert.Equal(this, self);

            Invoked = true;
        }

        public override string ToString() => GetHashCode().ToString();
    }

    [Fact]
    public void ConfigureTest()
    {
        static void Configure(IServiceProvider services, object startup) =>
            StartupLoader.Configure(services, startup,
                StartupLoader.FindMethod(startup.GetType(), nameof(Configure)));

        var startup = new ConfigureTestStartup2();
        var hostBuilder = new HostBuilder().ConfigureServices(s => s.AddSingleton(startup));

        var services = hostBuilder.Build().Services;

        Configure(services, new EmptyStartup());

        Assert.Throws<InvalidOperationException>(() => Configure(services, new ConfigureTestStartup0()));

        Assert.Throws<InvalidOperationException>(() => Configure(services, new ConfigureTestStartup1()));

        Configure(services, startup);

        Assert.True(startup.Invoked);
    }
    #endregion

    public class StaticStartup
    {
        public static int Created { get; set; }

        public StaticStartup() => Created++;

        public static void ConfigureServices(IServiceCollection services) { }

        public static void Configure(IServiceProvider provider) { }
    }

    public class StartupWithStaticMethod
    {
        public static int Created { get; set; }

        public StartupWithStaticMethod() => Created++;

        public void ConfigureServices(IServiceCollection services) { }

        public static void Configure(IServiceProvider provider) { }
    }

    [Fact]
    public void StaticMethodTest()
    {
        StartupLoader.CreateHost(typeof(StaticStartup), Name, diagnosticMessageSink);

        Assert.Equal(0, StaticStartup.Created);

        StartupLoader.CreateHost(typeof(StartupWithStaticMethod), Name, diagnosticMessageSink);

        Assert.Equal(1, StartupWithStaticMethod.Created);
    }
}
