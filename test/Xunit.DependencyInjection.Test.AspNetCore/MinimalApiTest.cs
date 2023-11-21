using Microsoft.Extensions.Hosting;
using MinimalApiSample;
using Xunit.DependencyInjection.AspNetCoreTesting;

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
        public IHostBuilder CreateHostBuilder() => MinimalApiHostBuilderFactory.GetHostBuilder<Program>();
    }
}
