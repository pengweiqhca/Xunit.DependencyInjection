using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestFramework : TestFramework
    {
        /// <inheritdoc />
        protected DependencyInjectionTestFramework(IMessageSink messageSink) : base(messageSink) { }

        /// <inheritdoc />
        protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo) =>
            new XunitTestFrameworkDiscoverer(assemblyInfo, SourceInformationProvider, DiagnosticMessageSink);

        /// <inheritdoc />
        protected sealed override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            IHost? host = null;
            try
            {
                var startup = StartupLoader.CreateStartup(assemblyName);
                if (startup == null) return new XunitTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);

                var hostBuilder = new HostBuilder();

                StartupLoader.ConfigureServices(hostBuilder, startup);

                host = hostBuilder
                    .ConfigureServices(services => services.AddSingleton<ITestOutputHelperAccessor, TestOutputHelperAccessor>())
                    .Build();

                StartupLoader.Configure(host.Services, startup);

                return new DependencyInjectionTestFrameworkExecutor(host, null,
                    assemblyName, SourceInformationProvider, DiagnosticMessageSink);
            }
            catch (Exception e)
            {
                return new DependencyInjectionTestFrameworkExecutor(host, e,
                    assemblyName, SourceInformationProvider, DiagnosticMessageSink);
            }
        }
    }
}
