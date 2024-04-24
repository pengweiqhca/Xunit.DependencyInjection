# Use `Microsoft.Extensions.DependencyInjection` to resolve xUnit test cases

## How to use

Install the [Nuget](https://www.nuget.org/packages/Xunit.DependencyInjection) package.

``` sh
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

### Asp.Net Core Startup

``` sh
dotnet add package Microsoft.AspNetCore.TestHost
```

``` C#
public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) => hostBuilder
        .ConfigureWebHost[Defaults](webHostBuilder => webHostBuilder
        .UseTestServer(options => options.PreserveExecutionContext = true)
        .UseStartup<AspNetCoreStartup>());
}
```

### MinimalApi

If you use MinimalApi rather than asp.net core Startup class.

Add package reference for `Xunit.DependencyInjection.AspNetCoreTesting`

``` sh
dotnet add package Xunit.DependencyInjection.AspNetCoreTesting
```

``` C#
public class Startup
{
    public IHostBuilder CreateHostBuilder() => MinimalApiHostBuilderFactory.GetHostBuilder<Program>();
}
```

> Maybe your asp.net core project should InternalsVisibleTo or add `public partial class Program {}` in the end of `Program.cs`;
>
> Detail see [Xunit.DependencyInjection.Test.AspNetCore](https://github.com/pengweiqhca/Xunit.DependencyInjection/tree/main/test/Xunit.DependencyInjection.Test.AspNetCore)

## `Startup` limitation

* `CreateHostBuilder` method

``` C#
public class Startup
{
    public IHostBuilder CreateHostBuilder([AssemblyName assemblyName]) { }
}
```

* `ConfigureHost` method

``` C#
public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) { }
}
```

* `ConfigureServices` method

``` C#
public class Startup
{
    public void ConfigureServices(IServiceCollection services[, HostBuilderContext context]) { }
}
```

* `Configure` method

Anything defined in `ConfigureServices`, can be specified in the `Configure` method signature. These services are injected if they're available.

## How to find `Startup`?

### 1. Specific startup

Declare [Startup] on test class

### 2. Nested startup

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

1. `A.B.C.Startup`
2. `A.B.Startup`
3. `A.Startup`
4. `Startup`

### 4. Default startup

> Default startup is required before 8.7.0, is optional in some case after 8.7.0.
>
> If is required, please add a startup class in your test project.

Default is find `Your.Test.Project.Startup, Your.Test.Project`.

If you want to use a special `Startup`, you can define `XunitStartupAssembly` and `XunitStartupFullName` in the `PropertyGroup` section

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

## Parallel

By default, xUnit runs all test cases in a test class synchronously. This package can extend the test framework to execute tests in parallel.

``` xml
<Project>

  <PropertyGroup>
    <ParallelizationMode></ParallelizationMode>
  </PropertyGroup>

</Project>
```

This package has two policies to run test cases in parallel.

1. Enhance or true

   Respect xunit [parallelization](https://xunit.net/docs/running-tests-in-parallel) behavior.

2. Force

   Ignore xunit [parallelization](https://xunit.net/docs/running-tests-in-parallel) behavior and force running tests in parallel.

If [`[Collection]`](https://github.com/xunit/xunit/issues/1227#issuecomment-297131879)(if ParallelizationMode is not `Force`), `[CollectionDefinition(DisableParallelization = true)]`, `[DisableParallelization]` declared on the test class, the test class will run sequentially. If `[DisableParallelization]`, `[MemberData(DisableDiscoveryEnumeration = true)]` declared on the test method, the test method will run sequentially.

> Thanks [Meziantou.Xunit.ParallelTestFramework](https://github.com/meziantou/Meziantou.Xunit.ParallelTestFramework)

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

## Write `Microsoft.Extensions.Logging` to `ITestOutputHelper`

Add package reference for `Xunit.DependencyInjection.Logging`

```sh
dotnet add package Xunit.DependencyInjection.Logging
```

> The call chain must be from the test case. If not, this feature will not work.

``` C#
public class Startup
{
    public void ConfigureServices(IServiceCollection services) => services
        .AddLogging(lb => lb.AddXunitOutput());
}
```

## How to inject `IConfiguration` or `IHostEnvironment` into `Startup`?

``` C#
public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) => hostBuilder
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
    public void ConfigureHost(IHostBuilder hostBuilder) => hostBuilder
        .ConfigureHostConfiguration(builder => { })
        .ConfigureAppConfiguration((context, builder) => { });
}
```

## [MemberData] how to inject?

Use **[MethodData]**

## Integrate OpenTelemetry

``` C#
TracerProviderBuilder builder;

builder.AddSource("Xunit.DependencyInjection");
```

## Do something before and after test case

Inherit `BeforeAfterTest` and register as `BeforeAfterTest` service.

[See demo](https://github.com/pengweiqhca/Xunit.DependencyInjection/blob/main/test/Xunit.DependencyInjection.Test/BeforeAfterTestTest.cs#13).
