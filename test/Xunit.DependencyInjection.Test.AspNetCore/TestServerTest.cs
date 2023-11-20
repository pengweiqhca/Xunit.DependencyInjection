using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;

namespace Xunit.DependencyInjection.Test.AspNetCore;

public class TestServerTest(HttpClient httpClient)
{
    public static string Key { get; } = Guid.NewGuid().ToString("N");

    [Fact]
    public async Task HttpTest()
    {
        using var response = await httpClient.GetAsync("/");

        response.EnsureSuccessStatusCode();

        Assert.Equal(Key, await response.Content.ReadAsStringAsync());
    }
}
