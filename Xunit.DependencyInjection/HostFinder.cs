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

        private IHost? FindHostForTestCaseType(Type type)
        {
            if (!type.IsNested) return AssemblyStartupHost;
            foreach (var hostAndModule in HostsAndModules)
            {
                var declaringType = type.DeclaringType;
                if (declaringType == hostAndModule.ModuleType)
                    return hostAndModule.Host; }

            return AssemblyStartupHost;
        }

        public IHost? GetHostForTestCase(IXunitTestCase testCase)
        {
            return FindHostForTestCaseType(testCase.Method.ToRuntimeMethod().DeclaringType);
        }

        public IHost? GetHostForTestFixture(Type fixture)
        {
            return FindHostForTestCaseType(fixture);
        }
    }
}
