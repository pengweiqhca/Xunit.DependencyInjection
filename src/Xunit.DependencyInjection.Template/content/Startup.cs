using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TProject
{
    public class Startup
    {
        // create custom hostBuilder with this method
        // public IHostBuilder CreateHostBuilder()
        // {
        //     // minimal API testing, see details: https://github.com/pengweiqhca/Xunit.DependencyInjection#minimalapi
        //     return MinimalApiHostBuilderFactory.GetHostBuilder<Program>();
        // }

        // custom host build
        public void ConfigureHost(IHostBuilder hostBuilder)
        {
            // hostBuilder
            //     .ConfigureHostConfiguration(builder =>
            //     {
            //         builder.AddInMemoryCollection(new Dictionary<string, string>()
            //             {
            //                 { "UserName", "Alice" }
            //             });
            //     });
        }

        // add services need to injection
        // ConfigureServices(IServiceCollection services)
        // ConfigureServices(IServiceCollection services, HostBuilderContext hostBuilderContext)
        // ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
        public void ConfigureServices(IServiceCollection services, HostBuilderContext hostBuilderContext)
        {
            // get configuration by hostBuilderContext.Configuration
            // eg: hostBuilderContext.Configuration["Environment"]

            //services.AddSingleton<CustomService>();
        }

        // Initialize logic, executed before running test cases
        public void Configure(IServiceProvider applicationServices)
        {
        }
    }
}
