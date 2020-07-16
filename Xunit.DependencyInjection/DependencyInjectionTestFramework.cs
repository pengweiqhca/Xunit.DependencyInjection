using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public sealed class DependencyInjectionTestFramework : XunitTestFramework
    {
        public DependencyInjectionTestFramework(IMessageSink messageSink) : base(messageSink)
        {
        }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            IHost? host = null;
            try
            {
                var startup = StartupLoader.CreateStartup(StartupLoader.GetStartupType(assemblyName), assemblyName);
                if (startup == null) return new XunitTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);

                var hostBuilder = StartupLoader.ConfigureHost(new HostBuilder()
                    .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection(new Dictionary<string, string> { { HostDefaults.ApplicationKey, assemblyName.Name! } })), startup);

                StartupLoader.ConfigureServices(hostBuilder, startup);

                host = hostBuilder
                    .ConfigureServices(services => services
                        .AddSingleton(DiagnosticMessageSink)
                        .TryAddSingleton<ITestOutputHelperAccessor, TestOutputHelperAccessor>()
                        )
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
