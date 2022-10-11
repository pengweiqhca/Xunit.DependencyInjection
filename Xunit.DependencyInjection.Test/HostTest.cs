using Autofac.Extensions.DependencyInjection;

namespace Xunit.DependencyInjection.Test;

public class HostTest
{
    private readonly IHostEnvironment _environment;
    private readonly IServiceProvider _provider;

    public HostTest(IHostEnvironment environment, IServiceProvider provider)
    {
        _environment = environment;
        _provider = provider;
    }

    [Fact]
    public void ApplicationNameTest() => Assert.Equal(typeof(HostTest).Assembly.GetName().Name, _environment.ApplicationName);

    [Fact]
    public void IsAutofac() => Assert.IsType<AutofacServiceProvider>(_provider);
}
