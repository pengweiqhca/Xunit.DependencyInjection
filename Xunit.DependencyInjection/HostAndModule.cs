using System;
using Microsoft.Extensions.Hosting;

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
}
