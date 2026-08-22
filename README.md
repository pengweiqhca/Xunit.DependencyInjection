# Xunit.DependencyInjection

[![Xunit.DependencyInjection NuGet](https://img.shields.io/nuget/v/Xunit.DependencyInjection)](https://www.nuget.org/packages/Xunit.DependencyInjection) [![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/pengweiqhca/xunit.dependencyinjection)

Use `Microsoft.Extensions.DependencyInjection` to resolve xUnit test cases: constructor-inject services into your test classes instead of writing them by hand, and reuse the same `Startup`/host configuration you use in your application.

> xUnit v2 users: please use the [v2](https://github.com/pengweiqhca/Xunit.DependencyInjection/tree/v2) branch.
>
> `Xunit.DependencyInjection.SkippableFact` is obsolete on xunit.v3 and no longer needed.

## Getting started

Install the [NuGet](https://www.nuget.org/packages/Xunit.DependencyInjection) package:

```sh
dotnet add package Xunit.DependencyInjection
dotnet add package xunit.v3 --version 4.0.0
```

> xUnit v4 uses the `xunit.v3` package. When upgrading, replace any `xunit.v3.mtp-v2` reference with `xunit.v3`.

Add a `Startup` class to your test project and register your services in `ConfigureServices`:

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

Then inject `IDependency` into your test class constructor, exactly like you would with any other DI-enabled class:

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

>  `Xunit.DependencyInjection` builds on top of the generic host and fully supports its lifecycle, so you can use any feature the generic host offers, including (but not limited to) `IHostedService`.

## Integrating with ASP.NET Core TestHost (3.0+)

### With an ASP.NET Core `Startup` class

```sh
dotnet add package Microsoft.AspNetCore.TestHost
```

```C#
public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) => hostBuilder
        .ConfigureWebHost[Defaults](webHostBuilder => webHostBuilder
        .UseTestServer(options => options.PreserveExecutionContext = true)
        .UseStartup<AspNetCoreStartup>());
}
```

### With Minimal APIs

If your web project uses Minimal APIs instead of an ASP.NET Core `Startup` class, install `Xunit.DependencyInjection.AspNetCoreTesting`:

```sh
dotnet add package Xunit.DependencyInjection.AspNetCoreTesting
```

```C#
public class Startup
{
    public IHostBuilder CreateHostBuilder() => MinimalApiHostBuilderFactory.GetHostBuilder<Program>();
}
```

> Your ASP.NET Core project may need to add `InternalsVisibleTo` for the test project, or add `public partial class Program { }` at the end of `Program.cs`, so the test project can reference `Program`.
>
> See [Xunit.DependencyInjection.Test.AspNetCore](https://github.com/pengweiqhca/Xunit.DependencyInjection/tree/main/test/Xunit.DependencyInjection.Test.AspNetCore) for a full example.

## `Startup` configuration styles

`Startup` supports two configuration styles. The `Configure` method (see [Initializing data on startup](#initializing-data-on-startup)) is supported by both styles.

### `HostApplicationBuilder` style

* `CreateHostApplicationBuilder` method

  > If this method is not found, the host falls back to `Host.CreateEmptyApplicationBuilder(new() { ApplicationName = assemblyName.Name })`.

  ```C#
  public HostApplicationBuilder CreateHostApplicationBuilder([AssemblyName assemblyName]) { }
  ```

* `ConfigureHostApplicationBuilder` method (presence of this method selects the `HostApplicationBuilder` style)

  ```C#
  public void ConfigureHostApplicationBuilder(IHostApplicationBuilder hostApplicationBuilder) { }
  ```

* `BuildHostApplicationBuilder` method

  > If this method is not found, the host is built by simply calling `hostApplicationBuilder.Build()`.

  ```C#
  public IHost BuildHostApplicationBuilder(HostApplicationBuilder hostApplicationBuilder)
  {
      return hostApplicationBuilder.Build();
  }
  ```

### `Startup`/`HostBuilder` style

* `CreateHostBuilder` method

  ```C#
  public class Startup
  {
      public IHostBuilder CreateHostBuilder([AssemblyName assemblyName]) { }
  }
  ```

* `ConfigureHost` method

  ```C#
  public class Startup
  {
      public void ConfigureHost(IHostBuilder hostBuilder) { }
  }
  ```

* `ConfigureServices` method

  ```C#
  public class Startup
  {
      public void ConfigureServices(IServiceCollection services[, HostBuilderContext context]) { }
  }
  ```

* `BuildHost` method

  > If this method is not found, the host is built by simply calling `hostBuilder.Build()`.

  ```C#
  public class Startup
  {
      public IHost BuildHost([IHostBuilder hostBuilder]) { return hostBuilder.Build(); }
  }
  ```

Method parameters wrapped in `[...]` above are optional.

## How is `Startup` located?

Startup classes are looked up in the following order; the first match wins.

### 1. Startup declared on the test class

Apply `[Startup(typeof(MyStartup))]` on the test class.

### 2. Nested `Startup`

```C#
public class TestClass1
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) { }
    }
}
```

### 3. Closest `Startup` in the namespace hierarchy

If the test class's full name is `A.B.C.TestClass`, `Startup` is looked up in this order:

1. `A.B.C.Startup`
2. `A.B.Startup`
3. `A.Startup`
4. `Startup`

### 4. Default `Startup`

> A default `Startup` was required before 8.7.0, and is optional in some cases after 8.7.0. When it's required, add a startup class to your test project as shown above.

By default, `Your.Test.Project.Startup, Your.Test.Project` is used.

If you want to use a custom `Startup`, set `XunitStartupAssembly` and/or `XunitStartupFullName` in your project's `PropertyGroup`:

```xml
<Project>
  <PropertyGroup>
    <XunitStartupAssembly>Abc</XunitStartupAssembly>
    <XunitStartupFullName>Xyz</XunitStartupFullName>
  </PropertyGroup>
</Project>
```

| XunitStartupAssembly | XunitStartupFullName | Resulting `Startup`                          |
| --------------------- | --------------------- | --------------------------------------------- |
|                       |                       | `Your.Test.Project.Startup, Your.Test.Project` |
| `Abc`                 |                       | `Abc.Startup, Abc`                             |
|                       | `Xyz`                 | `Xyz, Your.Test.Project`                       |
| `Abc`                 | `Xyz`                 | `Xyz, Abc`                                     |

## Running tests in parallel

By default, xUnit runs tests from different test collections in parallel, while tests in the same class run sequentially. xUnit v4 supports three parallelization modes:

1. `ParallelMode.None`: Run all tests sequentially.
2. `ParallelMode.Collections` (default): Run different test collections in parallel.
3. `ParallelMode.All`: Run all tests in parallel, including tests in the same class.

Configure the mode with xUnit's assembly-level `Parallelization` attribute:

```C#
using Xunit.v3;

[assembly: Parallelization(MaxThreads = 2, Mode = ParallelMode.All)]
```

`MaxThreads` is optional; set it to limit the number of tests running concurrently. Remove the `ParallelizationMode` MSBuild property when upgrading, as it no longer controls parallelization.

> If you register a custom `ITestCollectionOrderer`, test collections run in the order it specifies, which can be slower than running without one.

To run tests in a class or method sequentially when using `ParallelMode.All`, decorate it with `[DisableParallelization]`. To prevent a test collection from running in parallel with other tests, use `[CollectionDefinition(DisableParallelization = true)]`.

See xUnit's [parallelization documentation](https://xunit.net/docs/running-tests-in-parallel) for `Parallelization.Algorithm` and runner-specific configuration.

> Thanks to [Meziantou.Xunit.ParallelTestFramework](https://github.com/meziantou/Meziantou.Xunit.ParallelTestFramework) for the inspiration.

## Disabling Xunit.DependencyInjection

```xml
<Project>
    <PropertyGroup>
        <EnableXunitDependencyInjectionDefaultTestFrameworkAttribute>false</EnableXunitDependencyInjectionDefaultTestFrameworkAttribute>
    </PropertyGroup>
</Project>
```

## Injecting `ITestOutputHelper`

Inject `ITestOutputHelperAccessor` instead of `ITestOutputHelper` directly, since the actual instance is only available while a test is running:

```C#
internal class DependencyClass : IDependency
{
    private readonly ITestOutputHelperAccessor _testOutputHelperAccessor;

    public DependencyClass(ITestOutputHelperAccessor testOutputHelperAccessor)
    {
        _testOutputHelperAccessor = testOutputHelperAccessor;
    }
}
```

## Writing `Microsoft.Extensions.Logging` output to `ITestOutputHelper`

Install `Xunit.DependencyInjection.Logging`:

```sh
dotnet add package Xunit.DependencyInjection.Logging
```

> The call chain must originate from the running test case; otherwise this feature won't work.

```C#
public class Startup
{
    public void ConfigureServices(IServiceCollection services) => services
        .AddLogging(lb => lb.AddXunitOutput());
}
```

## Injecting `IConfiguration` or `IHostEnvironment` into `Startup`

```C#
public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) => hostBuilder
        .ConfigureServices((context, services) => { /* use context.Configuration / context.HostingEnvironment */ });
}
```

or

```C#
public class Startup
{
    public void ConfigureServices(IServiceCollection services, HostBuilderContext context)
    {
        // use context.Configuration / context.HostingEnvironment
    }
}
```

## Customizing `IConfiguration`

```C#
public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) => hostBuilder
        .ConfigureHostConfiguration(builder => { })
        .ConfigureAppConfiguration((context, builder) => { });
}
```

## How do I inject values with `[MemberData]`?

`[MemberData]` members are static and can't be resolved from the container, so use **[MethodData]** instead — it resolves the referenced method's parameters from DI.

## Integrating with OpenTelemetry

Register the `Xunit.DependencyInjection` activity source with your `TracerProviderBuilder` to capture the spans this library emits:

```C#
TracerProviderBuilder builder;

builder.AddSource("Xunit.DependencyInjection");
```

## Running code before and after each test

Inherit from `BeforeAfterTest` and register your implementation as a `BeforeAfterTest` service.

[See the sample](https://github.com/pengweiqhca/Xunit.DependencyInjection/blob/main/test/Xunit.DependencyInjection.Test/BeforeAfterTestTest.cs#13).

## Initializing data on startup

For synchronous initialization, use the `Configure` method. For asynchronous initialization, use an `IHostedService`.

## Related packages

| Package | Description |
| --- | --- |
| [Xunit.DependencyInjection.Logging](https://www.nuget.org/packages/Xunit.DependencyInjection.Logging) | Write `Microsoft.Extensions.Logging` output to `ITestOutputHelper`, see [above](#writing-microsoftextensionslogging-output-to-itestoutputhelper) |
| [Xunit.DependencyInjection.AspNetCoreTesting](https://www.nuget.org/packages/Xunit.DependencyInjection.AspNetCoreTesting) | Integration with ASP.NET Core Minimal API TestHost, see [above](#with-minimal-apis) |
| [Xunit.DependencyInjection.StaFact](https://www.nuget.org/packages/Xunit.DependencyInjection.StaFact) | Run `[StaFact]`/`[StaTheory]` test cases on an STA thread (e.g. for UI tests) |
| [Xunit.DependencyInjection.xRetry](https://www.nuget.org/packages/Xunit.DependencyInjection.xRetry) | Support [xRetry](https://github.com/JoshKeegan/xRetry)'s `[RetryFact]`/`[RetryTheory]` |
| [Xunit.DependencyInjection.FsCheck](https://www.nuget.org/packages/Xunit.DependencyInjection.FsCheck) | Support FsCheck property-based `[Property]` tests |
| [Xunit.DependencyInjection.Demystifier](https://www.nuget.org/packages/Xunit.DependencyInjection.Demystifier) | Use [Ben.Demystifier](https://github.com/benaadams/Ben.Demystifier) to format exception stack traces |
| [Xunit.DependencyInjection.Analyzer](https://www.nuget.org/packages/Xunit.DependencyInjection.Analyzer) | Roslyn analyzer that validates `Startup` class shape at compile time |
| [Xunit.DependencyInjection.Template](https://www.nuget.org/packages/Xunit.DependencyInjection.Template) | `dotnet new xunit-di` template to scaffold a new test project |

### StaFact

```sh
dotnet add package Xunit.DependencyInjection.StaFact
```

```C#
public class Startup
{
    public void ConfigureServices(IServiceCollection services) => services.AddStaFactSupport();
}
```

```C#
public class MyStaTests
{
    [StaFact]
    public void RunOnStaThread() { }

    [StaTheory]
    [InlineData(1)]
    public void RunOnStaThread(int value) { }
}
```

### xRetry

```sh
dotnet add package Xunit.DependencyInjection.xRetry
```

```C#
public class Startup
{
    public void ConfigureServices(IServiceCollection services) => services.AddXRetrySupport();
}
```

```C#
public class MyRetryTests
{
    [RetryFact(3)]
    public void FlakyTest() { }
}
```

### FsCheck

```sh
dotnet add package Xunit.DependencyInjection.FsCheck
```

```C#
public class Startup
{
    public void ConfigureServices(IServiceCollection services) => services.AddFsCheckSupport();
}
```

### Demystifier

```sh
dotnet add package Xunit.DependencyInjection.Demystifier
```

```C#
public class Startup
{
    public void ConfigureServices(IServiceCollection services) => services.UseDemystifyExceptionFilter();
}
```

### Analyzer

The analyzer is automatically added as an analyzer reference when you install `Xunit.DependencyInjection`, and reports compile-time diagnostics (e.g. multiple `Startup` constructors, invalid `Configure*` method signatures) so misconfigured `Startup` classes are caught early.

### Project template

```sh
dotnet new install Xunit.DependencyInjection.Template
dotnet new create xunit-di -n MyTestProject
```

See [Xunit.DependencyInjection.Template](https://github.com/pengweiqhca/Xunit.DependencyInjection/tree/main/src/Xunit.DependencyInjection.Template) for details.
