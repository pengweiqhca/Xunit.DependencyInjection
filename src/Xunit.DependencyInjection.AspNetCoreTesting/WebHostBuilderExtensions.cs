using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Xunit.DependencyInjection.AspNetCoreTesting;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder UseTestServerAndAddDefaultHttpClient(this IWebHostBuilder webHostBuilder) =>
        webHostBuilder.UseTestServerAndAddDefaultHttpClient(x => x.PreserveExecutionContext = true);

    public static IWebHostBuilder UseTestServerAndAddDefaultHttpClient(this IWebHostBuilder webHostBuilder,
        Action<TestServerOptions> testServerConfigure)
    {
        ArgumentNullException.ThrowIfNull(testServerConfigure);

        webHostBuilder.UseTestServer(testServerConfigure);
        webHostBuilder.ConfigureServices(x =>
        {
            x.TryAddSingleton<ITestClientWrapper>(sp => new TestClientWrapper(
                sp.GetRequiredService<IHost>().GetTestServer()));
            x.TryAddSingleton<HttpClient>(sp =>
                sp.GetRequiredService<ITestClientWrapper>().TestClient);
        });

        return webHostBuilder;
    }
}
