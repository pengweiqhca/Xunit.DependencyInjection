using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.DependencyInjection.AspNetCoreTesting;

namespace Xunit.DependencyInjection.Test.AspNetCore;

public class WebApplicationFactoryTest
{
    [Fact]
    public async Task HttpTest()
    {
        var factory = new XunitWebApplicationFactory<Startup>();

        await using var disposable = factory.ConfigureAwait(false);

        var httpClient = factory.Server.CreateClient();

        using var response = await httpClient.GetAsync("/").ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        Assert.Equal(TestServerTest.Key, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
    }
}
