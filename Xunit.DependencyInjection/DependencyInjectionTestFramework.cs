using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public sealed class DependencyInjectionTestFramework : XunitTestFramework
    {
        public DependencyInjectionTestFramework(IMessageSink messageSink) : base(messageSink) { }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            IHost? host = null;
            try
            {
                var startup = StartupLoader.CreateStartup(assemblyName);
                if (startup == null) return new XunitTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);

                var hostBuilder = StartupLoader.ConfigureHost(new HostBuilder(), startup);

                StartupLoader.ConfigureServices(hostBuilder, startup);

                host = hostBuilder
                   .ConfigureServices(services =>
                        services
                            .AddSingleton<ITestOutputHelperAccessor, TestOutputHelperAccessor>()
                            .AddSingleton<IMessageSink>(DiagnosticMessageSink))
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
