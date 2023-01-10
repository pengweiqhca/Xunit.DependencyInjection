using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection.Logging;

namespace Xunit.DependencyInjection.Test.AspNetCore;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) =>
        hostBuilder.ConfigureWebHost(webHostBuilder => webHostBuilder
#if NET
            .UseTestServer(options => options.PreserveExecutionContext = true)
#else
            .UseTestServer()
#endif
            .UseStartup<AspNetCoreStartup>());

    private class AspNetCoreStartup
    {
        public void Configure(IApplicationBuilder app)
        {
#if NETCOREAPP3_1
            ((TestServer)app.ApplicationServices.GetRequiredService<IServer>()).PreserveExecutionContext = true;
#endif
            XunitTestOutputLoggerProvider.Register(app.ApplicationServices);

            app.Run(static context => context.Response.WriteAsync(TestServerTest.Key));
        }
    }
}
