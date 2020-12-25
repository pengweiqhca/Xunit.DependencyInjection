using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection.Demystifier;
using Xunit.DependencyInjection.Logging;

namespace Xunit.DependencyInjection.Test
{
    public partial class Startup
    {
        public void ConfigureHost(IHostBuilder hostBuilder) =>
            hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

        public void ConfigureServices(IServiceCollection services) =>
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Debug))
                .AddScoped<IDependency, DependencyClass>()
                .AddScoped<IDependencyWithManagedLifetime, DependencyWithManagedLifetime>()
                .AddHostedService<HostServiceTest>()
                .AddSingleton<IAsyncExceptionFilter, DemystifyExceptionFilter>();

        public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor accessor) =>
            loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(accessor, delegate { return true; }));
    }
}
