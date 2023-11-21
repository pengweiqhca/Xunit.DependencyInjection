using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Xunit.DependencyInjection.AspNetCoreTesting;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder UseTestServerAndClient(this IWebHostBuilder webHostBuilder)
    {
        ArgumentNullException.ThrowIfNull(webHostBuilder);

        webHostBuilder.UseTestServer(x => x.PreserveExecutionContext = true);
        webHostBuilder.ConfigureServices(x =>
        {
            x.TryAddSingleton<HttpClient>(sp =>
                ((TestServer)sp.GetRequiredService<IServer>()).CreateClient());
        });

        return webHostBuilder;
    }

    public static IWebHostBuilder UseTestServerAndClient(this IWebHostBuilder webHostBuilder,
        Action<TestServerOptions> testServerConfigure)
    {
        ArgumentNullException.ThrowIfNull(testServerConfigure);

        webHostBuilder.UseTestServerAndClient()
            .ConfigureServices(x => x.Configure(testServerConfigure));

        return webHostBuilder;
    }
}
