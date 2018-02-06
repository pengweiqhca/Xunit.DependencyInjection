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

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            var services = new ServiceCollection();

            ConfigureServices(services);

            return new DependencyInjectionTestFrameworkExecutor(GetServiceProvider(services),
                assemblyName, SourceInformationProvider, DiagnosticMessageSink);
        }

        protected abstract void ConfigureServices(IServiceCollection services);

        /// <summary>You can use autofac or other</summary>
        protected virtual IServiceProvider GetServiceProvider(IServiceCollection services) =>
            services.BuildServiceProvider();
    }
}
