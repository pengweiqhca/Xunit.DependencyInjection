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
                host = CreateHost(assemblyName);

                if (host == null) return new XunitTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
            }
            catch (TargetInvocationException tie)
            {
                ex = tie.InnerException;
            }
            catch (Exception e)
            {
                ex = e;
            }

            return new DependencyInjectionTestFrameworkExecutor(host, ex,
                assemblyName, SourceInformationProvider, DiagnosticMessageSink);
        }

        private IHost? CreateHost(AssemblyName assemblyName)
        {
            var startup = StartupLoader.CreateStartup(StartupLoader.GetStartupType(assemblyName));
            if (startup == null) return null;

            var hostBuilder = StartupLoader.CreateHostBuilder(startup, assemblyName) ?? new HostBuilder();

            hostBuilder.ConfigureHostConfiguration(builder => builder.AddInMemoryCollection(
                new Dictionary<string, string> { { HostDefaults.ApplicationKey, assemblyName.Name } }));

            StartupLoader.ConfigureHost(hostBuilder, startup);

            StartupLoader.ConfigureServices(hostBuilder, startup);

            var host = hostBuilder.ConfigureServices(services =>
                {
                    services
                        .AddSingleton(DiagnosticMessageSink)
                        .TryAddSingleton<ITestOutputHelperAccessor, TestOutputHelperAccessor>();

                    services.TryAddEnumerable(ServiceDescriptor.Singleton<IXunitTestCaseRunnerWrapper, DependencyInjectionTestCaseRunnerWrapper>());
                    services.TryAddEnumerable(ServiceDescriptor.Singleton<IXunitTestCaseRunnerWrapper, DependencyInjectionTheoryTestCaseRunnerWrapper>());
                })
                .Build();

            StartupLoader.Configure(host.Services, startup);

            return host;
        }
    }
}
