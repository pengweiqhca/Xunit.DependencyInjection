using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public abstract class DependencyInjectionTestFramework : XunitTestFramework
    {
        protected DependencyInjectionTestFramework(IMessageSink messageSink) : base(messageSink) { }

        protected sealed override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            var services = new ServiceCollection();

            services.AddSingleton<ITestOutputHelperAccessor, TestOutputHelperAccessor>();

            var provider = ConfigureServices(services);

            using (var scope = provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                Configure(scope.ServiceProvider);

            return new DependencyInjectionTestFrameworkExecutor(provider,
                assemblyName, SourceInformationProvider, DiagnosticMessageSink);
        }

        protected abstract IServiceProvider ConfigureServices(IServiceCollection services);

        protected virtual void Configure(IServiceProvider provider) { }
    }
}
