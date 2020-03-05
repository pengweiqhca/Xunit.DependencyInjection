using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public abstract class DependencyInjectionTestFramework : TestFramework
    {
        /// <inheritdoc />
        protected DependencyInjectionTestFramework(IMessageSink messageSink) : base(messageSink) { }

        /// <inheritdoc />
        protected override ITestFrameworkDiscoverer CreateDiscoverer(
            IAssemblyInfo assemblyInfo) =>
            new XunitTestFrameworkDiscoverer(assemblyInfo, SourceInformationProvider, DiagnosticMessageSink);

        /// <inheritdoc />
        protected sealed override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            IHost? host = null;
            try
            {
                host = CreateHostBuilder(assemblyName)
                    .ConfigureServices(services => services.AddSingleton<ITestOutputHelperAccessor, TestOutputHelperAccessor>())
                    .Build();

                Configure(host.Services);

                return new DependencyInjectionTestFrameworkExecutor(host, null,
                    assemblyName, SourceInformationProvider, DiagnosticMessageSink);
            }
            catch (Exception e)
            {
                return new DependencyInjectionTestFrameworkExecutor(host, e,
                    assemblyName, SourceInformationProvider, DiagnosticMessageSink);
            }
        }

        /// <summary>Override this method to provide the implementation of <see cref="T:IHostBuilder" />.</summary>
        /// <param name="assemblyName">The assembly that is being executed.</param>
        /// <returns>Returns the host builder.</returns>
        protected virtual IHostBuilder CreateHostBuilder(AssemblyName assemblyName) => new HostBuilder();

        protected virtual void Configure(IServiceProvider provider) { }
    }
}
