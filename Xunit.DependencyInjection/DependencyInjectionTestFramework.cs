using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public sealed class HostAndModule
    {
        public IHost Host { get; }
        public Type? ModuleType { get; }

        public HostAndModule(IHost host, Type? moduleType)
        {
            Host = host;
            ModuleType = moduleType;
        }
    }

    public sealed class HostAndTestCase
    {
        public IHost? Host { get; }
        public List<IXunitTestCase> TestCases { get; }

        public HostAndTestCase(IHost? host,List<IXunitTestCase> testCases)
        {
            Host = host;
            TestCases = testCases;
        }
    }

    public sealed class HostData
    {
        public IHost? AssemblyStartupHost { get; }
        public HostAndModule[] HostsAndModules { get; }

        public HostData(IHost? assemblyStartupHost, HostAndModule[] hostsAndModules)
        {
            AssemblyStartupHost = assemblyStartupHost;
            HostsAndModules = hostsAndModules;
        }
    }

    public sealed class DependencyInjectionTestFramework : XunitTestFramework
    {
        public DependencyInjectionTestFramework(IMessageSink messageSink) : base(messageSink) { }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            HostData hostData = new HostData(null, Array.Empty<HostAndModule>());
            Exception? ex = null;

            try
            {
                hostData = CreateHost(assemblyName);

                if (hostData.AssemblyStartupHost is null && hostData.HostsAndModules.Length == 0) return new XunitTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
            }
            catch (TargetInvocationException tie)
            {
                ex = tie.InnerException;
            }
            catch (Exception e)
            {
                ex = e;
            }

            return new DependencyInjectionTestFrameworkExecutor(hostData, ex,
                assemblyName, SourceInformationProvider, DiagnosticMessageSink);
        }

        private HostData CreateHost(AssemblyName assemblyName)
        {
            IHost CreateStartupHost(object startupType)
            {
                var hostBuilder = StartupLoader.CreateHostBuilder(startupType, assemblyName) ?? new HostBuilder();

                hostBuilder.ConfigureHostConfiguration(builder => builder.AddInMemoryCollection(
                    new Dictionary<string, string> { { HostDefaults.ApplicationKey, assemblyName.Name } }));

                StartupLoader.ConfigureHost(hostBuilder, startupType);

                StartupLoader.ConfigureServices(hostBuilder, startupType);

                var host = hostBuilder.ConfigureServices(services =>
                                       {
                                           services
                                              .AddSingleton(DiagnosticMessageSink)
                                              .TryAddSingleton<ITestOutputHelperAccessor, TestOutputHelperAccessor>();

                                           services.TryAddEnumerable(ServiceDescriptor.Singleton<IXunitTestCaseRunnerWrapper, DependencyInjectionTestCaseRunnerWrapper>());
                                           services.TryAddEnumerable(ServiceDescriptor.Singleton<IXunitTestCaseRunnerWrapper, DependencyInjectionTheoryTestCaseRunnerWrapper>());
                                       })
                                      .Build();

                StartupLoader.Configure(host.Services, startupType);

                return host;
            }

            var assemblyStartupType = StartupLoader.GetAssemblyStartupType(assemblyName);
            var moduleStartupTypes = StartupLoader.GetModuleStartupTypes(assemblyStartupType);

            var assemblyStartup = StartupLoader.CreateStartup(assemblyStartupType);

            var moduleHosts =
                moduleStartupTypes.Select(x => (StartupLoader.CreateStartup(x.StartupType), x.ModuleType))
                                  .Where(x => x.Item1 is not null)
                                  .Select(x => (CreateStartupHost(x.Item1!), x.ModuleType))
                                  .Select(x => new HostAndModule(x.Item1, x.ModuleType))
                                  .ToArray();

            if (assemblyStartup is null)
                return new HostData(null, moduleHosts);

            var assemblyHost = CreateStartupHost(assemblyStartup);
            return new HostData(assemblyHost, moduleHosts);
        }
    }
}
