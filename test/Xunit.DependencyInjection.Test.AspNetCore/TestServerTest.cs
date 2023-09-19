using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;

namespace Xunit.DependencyInjection.Test.AspNetCore;

public class TestServerTest
{
    public static string Key { get; } = Guid.NewGuid().ToString("N");

    private readonly HttpClient _httpClient;

    public TestServerTest(IServer server) => _httpClient = ((TestServer)server).CreateClient();

    [Fact]
    public async Task HttpTest()
    {
        using var response = await _httpClient.GetAsync("/");

        response.EnsureSuccessStatusCode();

        Assert.Equal(Key, await response.Content.ReadAsStringAsync());
    }
}
