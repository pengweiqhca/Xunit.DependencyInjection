#if NETCOREAPP3_1
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Xunit.DependencyInjection.Demystifier;
using Xunit.DependencyInjection.Logging;

namespace Xunit.DependencyInjection.Test
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) =>
            services.AddLogging()
                .AddScoped<IDependency, DependencyClass>()
                .AddScoped<IDependencyWithManagedLifetime, DependencyWithManagedLifetime>()
                .AddHostedService<HostServiceTest>()
#if NETCOREAPP3_1
                .AddSingleton(provider => ((TestServer) provider.GetRequiredService<IServer>()).CreateClient())
#endif
                .AddSingleton<IAsyncExceptionFilter, DemystifyExceptionFilter>();
#if NETCOREAPP3_1
        public void ConfigureHost(IHostBuilder hostBuilder) =>
            hostBuilder.ConfigureWebHost(webHostBuilder => webHostBuilder
                .UseTestServer()
                .Configure(Configure)
                .ConfigureServices(services => services.AddRouting()));

        private void Configure(IApplicationBuilder app) =>
            app.UseRouting().Run(context => context.Response.WriteAsync(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()));
#endif
        public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor accessor) =>
            loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(accessor));
    }
}
