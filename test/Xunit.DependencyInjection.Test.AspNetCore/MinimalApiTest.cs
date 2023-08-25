using Microsoft.Extensions.Hosting;
using MinimalApiSample;
using Xunit.DependencyInjection.AspNetCoreTesting;

namespace Xunit.DependencyInjection.Test.AspNetCore;

public class MinimalApiTest
{
    private readonly HttpClient _httpClient;
    private readonly IRandomService _randomService;

    public MinimalApiTest(HttpClient httpClient, IRandomService randomService)
    {
        _httpClient = httpClient;
        _randomService = randomService;
    }

    [Fact]
    public async Task ResponseTest()
    {
        var responseText = await _httpClient.GetStringAsync("/").ConfigureAwait(false);
        Assert.Equal("Hello world", responseText);
    }

    [Fact]
    public void RandomTest()
    {
        var num = _randomService.Get();
        Assert.True(num < 10);
    }

    public class Startup
    {
        public IHostBuilder CreateHostBuilder() => MinimalApiHostBuilderFactory.GetHostBuilder<Program>();
    }
}
