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
        public DependencyInjectionTestFramework(IMessageSink messageSink) : base(messageSink) { }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            IHost? host = null;
            Exception? ex = null;
            try
            {
                var startup = StartupLoader.CreateStartup(StartupLoader.GetStartupType(assemblyName));
                if (startup == null) return new XunitTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);

                var hostBuilder = StartupLoader.CreateHostBuilder(startup, assemblyName) ??
                                  new HostBuilder().ConfigureHostConfiguration(builder =>
                                      builder.AddInMemoryCollection(new Dictionary<string, string> { { HostDefaults.ApplicationKey, assemblyName.Name } }));

                StartupLoader.ConfigureHost(hostBuilder, startup);

                StartupLoader.ConfigureServices(hostBuilder, startup);

                host = hostBuilder.ConfigureServices(services => services
                        .AddSingleton(DiagnosticMessageSink)
                        .TryAddSingleton<ITestOutputHelperAccessor, TestOutputHelperAccessor>())
                    .Build();

                StartupLoader.Configure(host.Services, startup);
            }
            catch (Exception e)
            {
                ex = e;
            }

            return new DependencyInjectionTestFrameworkExecutor(host, ex,
                assemblyName, SourceInformationProvider, DiagnosticMessageSink);
        }
    }
}
