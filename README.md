Use Microsoft.Extensions.DependencyInjection to resolve xUnit test cases.

How to use
=============

Install the [Nuget](https://www.nuget.org/packages/Xunit.DependencyInjection) package.

``` PS
Install-Package Xunit.DependencyInjection
```
In your testing project, add the following framework

```cs
[assembly: TestFramework("Your.Test.Project.ConfigureTestFramework", "AssemblyName")]

namespace Your.Test.Project
{
    public class ConfigureTestFramework : AutofacTestFramework
    {
        public ConfigureTestFramework(IMessageSink messageSink) : base(messageSink) { }

        protected override void ConfigureServices(IServiceCollection services)
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

How to use InstancePerTestCase(not test class)?
``` C#
public class InstancePerTest : IDisposable
{
    private readonly IServiceScope _serviceScope;
    private readonly IDependency _d;

    public InstancePerTest(IServiceProvider provider)
    {
        _serviceScope = provider.CreateScope();

        _d = _serviceScope.ServiceProvider.GetRequiredService<IDependency>();
    }

    public void Dispose() => _serviceScope.Dispose();
}
```