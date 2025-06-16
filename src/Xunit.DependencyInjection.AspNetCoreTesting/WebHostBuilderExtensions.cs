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
    public static IWebHostBuilder UseTestServer(this IWebHostBuilder webHostBuilder) => webHostBuilder
        .UseTestServer(x => x.PreserveExecutionContext = true)
        .ConfigureServices(x => x
            .AddSingleton<ITestHttpMessageHandlerFactory, TestServerHttpMessageHandlerFactory>());

    public static IWebHostBuilder UseUnixSocketServer(this IWebHostBuilder webHostBuilder) =>
        webHostBuilder.ConfigureServices(services =>
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetTempFileName()) + ".socket";

            services.Configure<KestrelServerOptions>(options => options.ListenUnixSocket(path))
                .AddSingleton<ITestHttpMessageHandlerFactory>(new UnixSocketHttpMessageHandlerFactory(path));
        });

    public static IWebHostBuilder AddTestHttpClient(this IWebHostBuilder webHostBuilder,
        params string[] httpClientNames) => webHostBuilder.ConfigureServices(x => x
            .AddSingleton<IConfigureOptions<HttpClientFactoryOptions>>(provider =>
                new ConfigureTestHttpClientFactoryOptions(provider.GetRequiredService<ITestHttpMessageHandlerFactory>(),
                    httpClientNames)))
        .ConfigureServices(services => services.AddHttpClient()
            .TryAddSingleton<HttpClient>(sp => sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient(httpClientNames.FirstOrDefault() ?? Options.DefaultName)));

    private sealed class ConfigureTestHttpClientFactoryOptions(ITestHttpMessageHandlerFactory factory, string[] names)
        : IConfigureNamedOptions<HttpClientFactoryOptions>
    {
        public void Configure(HttpClientFactoryOptions options) => Configure(Options.DefaultName, options);

        public void Configure(string? name, HttpClientFactoryOptions options)
        {
            if (names.Length < 1 ? name != Options.DefaultName : !names.Contains(name)) return;

            options.HttpClientActions.Add(client => client.BaseAddress ??= new("http://localhost"));

            options.HttpMessageHandlerBuilderActions.Add(x => x.PrimaryHandler = factory.CreateHandler());
        }
    }

    private sealed class TestServerHttpMessageHandlerFactory(IServer server) : ITestHttpMessageHandlerFactory
    {
        public HttpMessageHandler CreateHandler() => ((TestServer)server).CreateHandler();
    }

    private sealed class UnixSocketHttpMessageHandlerFactory(string path)
        : ITestHttpMessageHandlerFactory
    {
        public HttpMessageHandler CreateHandler() => new SocketsHttpHandler
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
        };
    }
}
