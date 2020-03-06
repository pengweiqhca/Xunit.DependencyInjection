Use Microsoft.Extensions.DependencyInjection to resolve xUnit test cases.

How to use
=============

Install the [Nuget](https://www.nuget.org/packages/Xunit.DependencyInjection) package.

``` PS
Install-Package Xunit.DependencyInjection
```
In your testing project, add the following framework

```cs
[assembly: TestFramework("Your.Test.Project.Startup", "AssemblyName")]

namespace Your.Test.Project
{
    public class Startup : DependencyInjectionTestFramework
    {
        public Startup(IMessageSink messageSink) : base(messageSink) { }

        protected void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDependency, DependencyClass>();
        }

        protected override IHostBuilder CreateHostBuilder(AssemblyName assemblyName) =>
            base.CreateHostBuilder(assemblyName)
                .ConfigureServices(ConfigureServices);
    }
}
```

Example test `class`.

```cs
public interface IDependency
{
    int Value { get; }
}

internal class DependencyClass : IDependency
{
    public int Value => 1;
}

public class MyAwesomeTests
{
    private readonly IDependency _d;

    public MyAwesomeTests(IDependency d) => _d = d;

    [Fact]
    public void AssertThatWeDoStuff()
    {
        Assert.Equal(1, _d.Value);
    }
}
```
## How to inject ITestOutputHelper
``` C#
internal class DependencyClass : IDependency
{
    private readonly ITestOutputHelperAccessor _testOutputHelperAccessor;

    public DependencyClass(ITestOutputHelperAccessor testOutputHelperAccessor)
    {
        _testOutputHelperAccessor = testOutputHelperAccessor;
    }
}
```

## Write Microsoft.Extensions.Logging to ITestOutputHelper
``` C#
    public class Startup : DependencyInjectionTestFramework
    {
        protected override void Configure(IServiceProvider provider)
        {
            provider.GetRequiredService<ILoggerFactory>()
                .AddProvider(new XunitTestOutputLoggerProvider(provider.GetRequiredService<ITestOutputHelperAccessor>()[, Func<string, LogLevel, bool> filter]));
        }
    }
```

## How to inject `IConfiguration` or `IHostingEnvironment` into `Startup`?
``` C#
    public class Startup : DependencyInjectionTestFramework
    {
        protected override IHostBuilder CreateHostBuilder(AssemblyName assemblyName) =>
            base.CreateHostBuilder(assemblyName)
                .ConfigureServices((context, services) => { context.XXXX });
    }
```

## How to configure `IConfiguration`?
``` C#
    public class Startup : DependencyInjectionTestFramework
    {
        protected override IHostBuilder CreateHostBuilder(AssemblyName assemblyName) =>
            Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration(builder => { })
                .ConfigureAppConfiguration((context, builder) => { });
    }
```
