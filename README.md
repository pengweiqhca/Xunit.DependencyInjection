Use Microsoft.Extensions.DependencyInjection to resolve xUnit test cases.

How to use
=============

Install the [Nuget](https://www.nuget.org/packages/Xunit.DependencyInjection) package.

``` PS
Install-Package Xunit.DependencyInjection
```
In your testing project, add the following framework

```cs
namespace Your.Test.Project
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDependency, DependencyClass>();
        }
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


## V7 new features

* Don't need to add the assembly attribute `TestFramework`.
* `Startup` does not need to inherit `DependencyInjectionTestFramework`.
* `Configure` method support multiple parameters, like asp.net core Startup.

## V6 to V7 break changes
``` diff

-[assembly: TestFramework("Your.Test.Project.Startup", "Your.Test.Project")]

namespace Your.Test.Project
{
-   public class Startup : DependencyInjectionTestFramework
+   public class Startup
    {
-       public Startup(IMessageSink messageSink) : base(messageSink) { }
+       public Startup(AssemblyName assemblyName) { }

-       protected void ConfigureServices(IServiceCollection services)
+       public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDependency, DependencyClass>();
        }

-       protected override IHostBuilder CreateHostBuilder() =>
-           base.CreateHostBuilder(assemblyName)
-               .ConfigureServices(ConfigureServices);
+       public void ConfigureHost(IHostBuilder hostBuilder) { }
    }
-       protected override void Configure(IServiceProvider provider)
+       public void Configure(IServiceProvider provider)
}
```

## `Startup` limitation

* Constructor
``` C#
public class Startup
{
    public Startup([AssemblyName assemblyName]) { }
}
```

* ConfigureHost method
``` C#
public class Startup
{
    public void/IHostBuilder ConfigureHost(IHostBuilder hostBuilder) { }
}
```

* ConfigureServices method
``` C#
public class Startup
{
    public void ConfigureServices(IServiceCollection services[, HostBuilderContext context]) { }
}
```

* Configure method
Anything defined in ConfigureServices, can be specified in the Configure method signature. These services are injected if they're available.

## How to find `Startup`?
Default is find `Your.Test.Project.Startup, Your.Test.Project`.
If you want use a special `Startup`, you can defined `XunitStartupAssembly` and `XunitStartupFullName` in `PropertyGroup` section
``` xml
<Project>
  <PropertyGroup>
    <XunitStartupAssembly>Abc</XunitStartupAssembly>
    <XunitStartupFullName>Xyz</XunitStartupFullName>
  </PropertyGroup>
</Project>
```
| XunitStartupAssembly | XunitStartupFullName | Startup |
| ------- | ------ | ------ |
|   |   | Your.Test.Project.Startup, Your.Test.Project |
| Abc |   | Abc.Startup, Abc |
|   | Xyz | Xyz, Your.Test.Project |
| Abc | Xyz | Xyz, Abc |

## V5 to V6 break changes
``` diff
namespace Your.Test.Project
{
    public class Startup : DependencyInjectionTestFramework
    {
        public Startup(IMessageSink messageSink) : base(messageSink) { }

-       protected override void ConfigureServices(IServiceCollection services)
+       protected void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDependency, DependencyClass>();
        }

+       protected override IHostBuilder CreateHostBuilder(AssemblyName assemblyName) =>
+           base.CreateHostBuilder(assemblyName)
+               .ConfigureServices(ConfigureServices);
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
    public class Startup
    {
        public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor accessor) =>
            loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(accessor));
    }
```

## How to inject `IConfiguration` or `IHostingEnvironment` into `Startup`?
``` C#
    public class Startup
    {
        public void ConfigureHost(IHostBuilder hostBuilder) =>
            hostBuilder
                .ConfigureServices((context, services) => { context.XXXX });
    }
```
or
``` C#
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services, HostBuilderContext context)
        {
            context.XXXX;
        }
    }
```

## How to configure `IConfiguration`?
``` C#
    public class Startup
    {
        public void ConfigureServices(IHostBuilder hostBuilder) =>
            hostBuilder
                .ConfigureHostConfiguration(builder => { })
                .ConfigureAppConfiguration((context, builder) => { });
    }
```
