using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public abstract class DependencyInjectionTestFramework : XunitTestFramework
    {
        protected IConfigurationRoot Root { get; private set; }

        protected DependencyInjectionTestFramework(IMessageSink messageSink) : base(messageSink) { }

        protected sealed override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            var builder = new ConfigurationBuilder();

            Configuration(builder);

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(Root = builder.Build())
                .AddSingleton<ITestOutputHelperAccessor, TestOutputHelperAccessor>();

            var provider = ConfigureServices(services);

            using (var scope = provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                Configure(scope.ServiceProvider);

            return new DependencyInjectionTestFrameworkExecutor(provider,
                assemblyName, SourceInformationProvider, DiagnosticMessageSink);
        }

        protected virtual void Configuration(IConfigurationBuilder builder) { }

        protected abstract IServiceProvider ConfigureServices(IServiceCollection services);

        protected virtual void Configure(IServiceProvider provider) { }
    }
}
