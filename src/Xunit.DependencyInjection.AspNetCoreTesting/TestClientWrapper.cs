using Microsoft.AspNetCore.TestHost;

namespace Xunit.DependencyInjection.AspNetCoreTesting;

public interface ITestClientWrapper
{
    TestServer TestServer { get; }
    HttpClient TestClient { get; }
}

internal sealed class TestClientWrapper(TestServer testServer) : ITestClientWrapper
{
    public TestServer TestServer => testServer;
    public HttpClient TestClient => testServer.CreateClient();
}
