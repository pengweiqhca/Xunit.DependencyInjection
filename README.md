Use Microsoft.Extensions.DependencyInjection to resolve xUnit test cases.

How to use
=============

Install the [Nuget](https://www.nuget.org/packages/Xunit.DependencyInjection) package.

``` bash
dotnet add package Xunit.DependencyInjection
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

## Integration asp.net core TestHost(3.0+)

``` bash
dotnet add package Microsoft.AspNetCore.TestHost
```

``` C#
public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) =>
        hostBuilder.ConfigureWebHost[Defaults](webHostBuilder => webHostBuilder
            .UseTestServer().UseStartup<AspNetCoreStartup>());
}
```

## `Startup` limitation

* CreateHostBuilder method
``` C#
public class Startup
{
    public IHostBuilder CreateHostBuilder([AssemblyName assemblyName]) { }
}
```

* ConfigureHost method
``` C#
public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) { }
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

### 1. Specific startup
Declare [Startup] on test class

### 2. Nest startup
``` C#
public class TestClass1
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) { }
    }
```

### 3. Closest startup
If the class type full name is "A.B.C.TestClass", find Startup in the following order:
1. A.B.C.Startup
2. A.B.Startup
3. A.Startup
4. Startup

### 4. Global startup
Default is find Your.Test.Project.Startup, Your.Test.Project`.

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
    public void ConfigureHost(IHostBuilder hostBuilder) =>
        hostBuilder
            .ConfigureHostConfiguration(builder => { })
            .ConfigureAppConfiguration((context, builder) => { });
}
```

## [MemberData] how to inject?
Use **[MethodData]**

## Integrate opentelemetry

``` C#
TracerProviderBuilder builder;

builder.AddSource("Xunit.DependencyInjection");
```
