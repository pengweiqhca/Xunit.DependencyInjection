using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using System.Net.Sockets;

namespace Xunit.DependencyInjection.AspNetCoreTesting;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder UseTestServerAndAddDefaultHttpClient(this IWebHostBuilder webHostBuilder) =>
        webHostBuilder.UseTestServerAndAddDefaultHttpClient(x => x.PreserveExecutionContext = true, []);

    public static IWebHostBuilder UseTestServerAndAddDefaultHttpClient(this IWebHostBuilder webHostBuilder,
        params string[] httpClientNames) =>
        webHostBuilder.UseTestServerAndAddDefaultHttpClient(x => x.PreserveExecutionContext = true, httpClientNames);

    public static IWebHostBuilder UseTestServerAndAddDefaultHttpClient(this IWebHostBuilder webHostBuilder,
        Action<TestServerOptions> testServerConfigure) =>
        webHostBuilder.UseTestServerAndAddDefaultHttpClient(testServerConfigure, []);

    public static IWebHostBuilder UseTestServerAndAddDefaultHttpClient(this IWebHostBuilder webHostBuilder,
        Action<TestServerOptions> testServerConfigure, params string[] httpClientNames)
    {
        ArgumentNullException.ThrowIfNull(testServerConfigure);

        webHostBuilder.UseTestServer(testServerConfigure)
            .ConfigureServices(x => x
                .AddSingleton<IConfigureOptions<HttpClientFactoryOptions>>(provider =>
                    new ConfigureTestServerHttpClientFactoryOptions(provider.GetRequiredService<IServer>(),
                        httpClientNames))
                .TryAddSingleton<HttpClient>(sp => ((TestServer)sp.GetRequiredService<IServer>()).CreateClient()));

        return webHostBuilder;
    }

    public static IWebHostBuilder UseUnixSocketServerAndAddDefaultHttpClient(this IWebHostBuilder webHostBuilder,
        params string[] httpClientNames) =>
        webHostBuilder.ConfigureServices(services =>
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetTempFileName()) + ".socket";

            if (httpClientNames.Length > 0 && !httpClientNames.Contains(Options.DefaultName))
                httpClientNames = httpClientNames.Append(Options.DefaultName).ToArray();

            services.Configure<KestrelServerOptions>(options => options.ListenUnixSocket(path))
                .AddSingleton<IConfigureOptions<HttpClientFactoryOptions>>(
                    new ConfigureUnixSocketHttpClientFactoryOptions(path, httpClientNames))
                .AddHttpClient()
                .TryAddSingleton<HttpClient>(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient());
        });

    private sealed class ConfigureTestServerHttpClientFactoryOptions(IServer server, params string[] names)
        : IConfigureNamedOptions<HttpClientFactoryOptions>
    {
        public void Configure(HttpClientFactoryOptions options) => Configure(Options.DefaultName, options);

        public void Configure(string? name, HttpClientFactoryOptions options)
        {
            if (names.Length > 0 && !names.Contains(name)) return;

            options.HttpMessageHandlerBuilderActions.Add(x => x.PrimaryHandler = ((TestServer)server).CreateHandler());
        }
    }

    private sealed class ConfigureUnixSocketHttpClientFactoryOptions(string path, params string[] names)
        : IConfigureNamedOptions<HttpClientFactoryOptions>
    {
        public void Configure(HttpClientFactoryOptions options) => Configure(Options.DefaultName, options);

        public void Configure(string? name, HttpClientFactoryOptions options)
        {
            if (names.Length > 0 && !names.Contains(name)) return;

            options.HttpClientActions.Add(client => client.BaseAddress = new("http://localhost"));

            options.HttpMessageHandlerBuilderActions.Add(x => x.PrimaryHandler = new SocketsHttpHandler
            {
                ConnectCallback = async (_, cancellationToken) =>
                {
                    var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

                    try
                    {
                        await socket.ConnectAsync(new UnixDomainSocketEndPoint(path), cancellationToken);

                        return new NetworkStream(socket, true);
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                }
            });
        }
    }
}
