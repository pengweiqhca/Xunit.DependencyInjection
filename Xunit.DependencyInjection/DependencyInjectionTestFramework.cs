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

    public sealed class DependencyInjectionTestFramework : XunitTestFramework
    {
        public DependencyInjectionTestFramework(IMessageSink messageSink) : base(messageSink) { }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            HostAndModule[] hostAndModules = Array.Empty<HostAndModule>();
            Exception? ex = null;

            try
            {
                hostAndModules = CreateHost(assemblyName);

                if (hostAndModules.Length == 0) return new XunitTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
            }
            catch (TargetInvocationException tie)
            {
                ex = tie.InnerException;
            }
            catch (Exception e)
            {
                ex = e;
            }

            return new DependencyInjectionTestFrameworkExecutor(hostAndModules, ex,
                assemblyName, SourceInformationProvider, DiagnosticMessageSink);
        }

        private HostAndModule[] CreateHost(AssemblyName assemblyName)
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
                                  .Select(x => new HostAndModule(x.Item1, x.ModuleType));

            if (assemblyStartup is null)
                return moduleHosts.ToArray();

            return new[] { new HostAndModule(CreateStartupHost(assemblyStartup), null) }.Concat(moduleHosts).ToArray();
        }
    }
}
