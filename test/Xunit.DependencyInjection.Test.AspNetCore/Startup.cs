using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using Xunit.DependencyInjection.AspNetCoreTesting;
using Xunit.DependencyInjection.Logging;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Xunit.DependencyInjection.Test.AspNetCore;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) =>
        hostBuilder.ConfigureWebHost(webHostBuilder => webHostBuilder
            .UseTestServerAndAddDefaultHttpClient()
            .UseStartup<AspNetCoreStartup>());

    private class AspNetCoreStartup
    {
        public void ConfigureServices(IServiceCollection services) => services.AddLogging(lb => lb.AddXunitOutput());

        public void Configure(IApplicationBuilder app) =>
            app.Run(static context => context.Response.WriteAsync(TestServerTest.Key));
    }
}
