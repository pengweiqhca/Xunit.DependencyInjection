using Microsoft.Extensions.DependencyInjection;

namespace Xunit.DependencyInjection.AspNetCoreTesting;

public static class HttpClientBuilderExtensions
{
    extension(IHttpClientBuilder builder)
    {
        public IHttpClientBuilder UseTestHandler() =>
            builder.ConfigurePrimaryHttpMessageHandler(static provider =>
                provider.GetRequiredService<ITestHttpMessageHandlerFactory>().CreateHandler());
    }
}
