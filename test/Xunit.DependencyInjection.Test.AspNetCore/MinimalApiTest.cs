using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinimalApiSample;
using Xunit.DependencyInjection.AspNetCoreTesting;
using Xunit.DependencyInjection.Logging;

namespace Xunit.DependencyInjection.Test.AspNetCore;

public class MinimalApiTest(HttpClient httpClient, IRandomService randomService)
{
    [Fact]
    public async Task MinimalApiRouteResponseTest()
    {
        var responseText = await httpClient.GetStringAsync("/hello");
        Assert.Equal("Hello world", responseText);
    }

    [Fact]
    public async Task ControllerRouteResponseTest()
    {
        var responseText = await httpClient.GetStringAsync("/");
        Assert.Equal("Hello world", responseText);
    }

    [Fact]
    public void RandomTest()
    {
        var num = randomService.Get();
        Assert.True(num < 10);
    }

    public class Startup
    {
        public IHostBuilder CreateHostBuilder() => MinimalApiHostBuilderFactory.GetHostBuilder<Program>()
            .ConfigureHostConfiguration(builder =>
                builder.AddInMemoryCollection([new(HostDefaults.EnvironmentKey, "Testing")]));

        public void ConfigureServices(IServiceCollection services) => services.AddLogging(lb => lb.AddXunitOutput());
    }
}
