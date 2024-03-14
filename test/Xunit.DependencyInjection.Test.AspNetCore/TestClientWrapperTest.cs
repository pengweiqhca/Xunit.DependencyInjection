using Xunit.DependencyInjection.AspNetCoreTesting;

namespace Xunit.DependencyInjection.Test.AspNetCore;

public class TestClientWrapperTest(ITestClientWrapper testClientWrapper)
{
    [Fact]
    public async Task TestClientTest()
    {
        Assert.NotNull(testClientWrapper.TestClient);

        using var response = await testClientWrapper.TestClient.GetAsync("/");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task TestServerTest()
    {
        Assert.NotNull(testClientWrapper.TestServer);

        var request = testClientWrapper.TestServer.CreateRequest("/");
        using var response = await request.GetAsync();
        response.EnsureSuccessStatusCode();
    }
}
