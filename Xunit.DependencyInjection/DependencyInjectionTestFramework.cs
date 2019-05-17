using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
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
            IHost host = null;
            try
            {
                host = CreateHostBuilder(assemblyName)
                    .ConfigureAppConfiguration((context, builder) =>
                    {
                        if (string.IsNullOrWhiteSpace(context.HostingEnvironment.ApplicationName))
                            builder.AddInMemoryCollection(new Dictionary<string, string>
                            {
                                {
                                    HostDefaults.ApplicationKey,
                                    context.HostingEnvironment.ApplicationName = assemblyName.Name
                                }
                            });
                    })
                    .ConfigureServices(services => services.AddSingleton<ITestOutputHelperAccessor, TestOutputHelperAccessor>())
                    .ConfigureServices(services => ConfigureServices(assemblyName, services))
                    .Build();

                using (var scope = host.Services.CreateScope())
                    Configure(scope.ServiceProvider);

                return new DependencyInjectionTestFrameworkExecutor(host, null,
                    assemblyName, SourceInformationProvider, DiagnosticMessageSink);
            }
            catch (Exception e)
            {
                return new DependencyInjectionTestFrameworkExecutor(host, e,
                    assemblyName, SourceInformationProvider, DiagnosticMessageSink);
            }
        }

        protected virtual IHostBuilder CreateHostBuilder(AssemblyName assembly) => new HostBuilder();

        protected virtual void ConfigureServices(AssemblyName assemblyName, IServiceCollection services) => ConfigureServices(services);

        protected abstract void ConfigureServices(IServiceCollection services);

        protected virtual void Configure(IServiceProvider provider) { }
    }
}
