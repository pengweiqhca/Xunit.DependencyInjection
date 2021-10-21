using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.DependencyInjection
{
    internal sealed class HostManager : IHostedService, IDisposable
    {
        private readonly IDictionary<Type, IHost> _hosts = new Dictionary<Type, IHost>();

        private IHost? _host;
        private readonly AssemblyName _assemblyName;
        private readonly IMessageSink _diagnosticMessageSink;

        public HostManager(AssemblyName assemblyName, IMessageSink diagnosticMessageSink)
        {
            _assemblyName = assemblyName;
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public IHost? BuildDefaultHost() => _host = StartupLoader.CreateHost(_assemblyName, _diagnosticMessageSink);

        public IHost? GetHost(Type? type)
        {
            if (type == null) return _host;

            var startupType = FindStartup(type);
            if (startupType == null) return _host;

            // ReSharper disable once InconsistentlySynchronizedField
            if (_hosts.TryGetValue(startupType, out var startup)) return startup;

            lock (_hosts)
            {
                if (_hosts.TryGetValue(startupType, out startup)) return startup;

                return _hosts[startupType] = StartupLoader.CreateHost(startupType, _assemblyName, _diagnosticMessageSink);
            }
        }

        private static Type? FindStartup(Type testClassType)
        {
            var attr = testClassType.GetCustomAttribute<StartupAttribute>();
            if (attr != null) return attr.StartupType;

            var type = testClassType;
            while(type != null)
            {
                var startupType = type.GetNestedType("Startup");
                if (startupType != null) return startupType;

                type = type.DeclaringType;
            }

            return null;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var tasks = Enumerable.Empty<Task>();
            if (_host != null) tasks = tasks.Union(Enumerable.Repeat(_host.StartAsync(cancellationToken), 1));

            tasks = tasks.Union(_hosts.Values
                .Where(x => x != null)
                .Select(x => x!.StartAsync(cancellationToken)));

            return Task.WhenAll(tasks);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            var tasks = _hosts.Values
                .Where(x => x != null)
                .Select(x => x!.StopAsync(cancellationToken));

            if (_host != null) tasks = tasks.Union(Enumerable.Repeat(_host.StopAsync(cancellationToken), 1));

            return Task.WhenAll(tasks);
        }

        public void Dispose()
        {
            foreach (var host in _hosts.Values) host?.Dispose();

            _host?.Dispose();
        }
    }
}
