using Microsoft.Extensions.DependencyInjection;

namespace Xunit.DependencyInjection.AspNetCoreTesting;

public static class HttpClientBuilderExtensions
{
    public static IHttpClientBuilder UseTestHandler(this IHttpClientBuilder builder) =>
        builder.ConfigurePrimaryHttpMessageHandler(static provider =>
            provider.GetRequiredService<ITestHttpMessageHandlerFactory>().CreateHandler());
}
