using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        webHostBuilder.ConfigureServices(x => x.TryAddSingleton<HttpClient>(sp =>
            ((TestServer)sp.GetRequiredService<IServer>()).CreateClient()));

        return webHostBuilder;
    }
}
