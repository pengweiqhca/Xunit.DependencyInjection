using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Xunit.DependencyInjection.Test.AspNetCore
{
    public class Startup
    {
        public void ConfigureHost(IHostBuilder hostBuilder) =>
            hostBuilder.ConfigureWebHost(webHostBuilder => webHostBuilder
                .UseTestServer()
                .Configure(Configure));

        private void Configure(IApplicationBuilder app) =>
            app.Run(context => context.Response.WriteAsync(TestServerTest.Key));
    }
}
