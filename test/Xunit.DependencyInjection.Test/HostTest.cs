using Autofac.Extensions.DependencyInjection;

namespace Xunit.DependencyInjection.Test;

public class HostTest(IHostEnvironment environment, IServiceProvider provider)
{
    [Fact]
    public void ApplicationNameTest() => Assert.Equal(typeof(HostTest).Assembly.GetName().Name, environment.ApplicationName);

    [Fact]
    public void IsAutofac() => Assert.IsType<AutofacServiceProvider>(provider);
}
