using System.Reflection;

namespace Xunit.DependencyInjection.Test;

public class CreateHostApplicationBuilderTest(IConfiguration configuration, IHostEnvironment environment)
{
    [Fact]
    public void AppSettingsTest() => Assert.Equal("testValue", configuration["testKey"]);

    [Fact]
    public void ApplicationNameTest() =>
        Assert.Equal(typeof(CreateHostApplicationBuilderTest).Assembly.GetName().Name, environment.ApplicationName);

    public class Startup : HostApplicationBuilderTest.Startup
    {
        public HostApplicationBuilder CreateHostApplicationBuilder(AssemblyName assemblyName) =>
            Host.CreateApplicationBuilder(new HostApplicationBuilderSettings()
            {
                ApplicationName = assemblyName.Name
            });
    }
}
