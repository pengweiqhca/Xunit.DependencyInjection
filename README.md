﻿Uses Microsoft.Extensions.DependencyInjection to resolve xUnit test cases.

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
            .UseTestServer(options => options.PreserveExecutionContext = true)
            .UseStartup<AspNetCoreStartup>());
}
```

> Detail see [Xunit.DependencyInjection.Test.AspNetCore](https://github.com/pengweiqhca/Xunit.DependencyInjection/tree/main/test/Xunit.DependencyInjection.Test.AspNetCore)

### Integration with `WebApplicationFactory`.

``` bash
dotnet add package Xunit.DependencyInjection.AspNetCoreTesting
```

``` C#
[Fact]
public async Task TestMethod()
{
    var factory = new XunitWebApplicationFactory<Startup>();
}
```

If you get a `System.IO.DirectoryNotFoundException`, please see [`SetContentRoot` source code](https://github.com/dotnet/aspnetcore/blob/a0db11ba7c3ae217e9745976056f10cb2a7dafde/src/Mvc/Mvc.Testing/src/WebApplicationFactory.cs#L216) and look for a proper solution.

Maybe I can give you a way:

1. Crate file `MvcTestingAppManifest.json`, and set `CopyToOutputDirectory="PreserveNewest"`

2. Add content

   ``` json
   {
     "FullName of the assembly that contains the Startup type": "~"
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

### 4. Default startup
> Default startup is required before 8.7.0, is optional in some case after 8.7.0.
>
> If is required, plaese add a startup class in your test project.

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

## How to disable Xunit.DependencyInjection
``` xml
<Project>
    <PropertyGroup>
        <EnableXunitDependencyInjectionDefaultTestFrameworkAttribute>false</EnableXunitDependencyInjectionDefaultTestFrameworkAttribute>
    </PropertyGroup>
</Project>
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
> The call chain must from test case. If not, this feature will not work.

``` C#
public class Startup
{
    public void ConfigureServices(IServiceCollection services) => services.AddLogging(lb => lb.AddXunitOutput());
}
```

## How to inject `IConfiguration` or `IHostEnvironment` into `Startup`?

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
