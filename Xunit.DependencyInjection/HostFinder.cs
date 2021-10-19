using System;
using Microsoft.Extensions.Hosting;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public sealed class HostFinder
    {
        public IHost? AssemblyStartupHost { get; }
        public HostAndModule[] HostsAndModules { get; }

        public HostFinder(IHost? assemblyStartupHost, HostAndModule[] hostsAndModules)
        {
            AssemblyStartupHost = assemblyStartupHost;
            HostsAndModules = hostsAndModules;
        }

        private IHost? FindHostFormTestCaseType(Type type)
        {
            if (!type.IsNested) return AssemblyStartupHost;
            for (int i = 0; i < HostsAndModules.Length; i++)
            {
                if (type.DeclaringType == HostsAndModules[i].ModuleType)
                    return HostsAndModules[i].Host;
            }

            return AssemblyStartupHost;
        }

        public IHost? GetHostForTestCase(IXunitTestCase testCase)
        {
            return FindHostFormTestCaseType(testCase.Method.ToRuntimeMethod().DeclaringType);
        }

        public IHost? GetHostForTestFixture(Type fixture)
        {
            return FindHostFormTestCaseType(fixture);
        }
    }
}
