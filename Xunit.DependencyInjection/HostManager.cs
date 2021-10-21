﻿using Microsoft.Extensions.Hosting;
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
        private readonly IDictionary<Type, IHost> _hostMap = new Dictionary<Type, IHost>();
        private readonly IList<IHost> _hosts = new List<IHost>();

        private Type? _defaultStartupType;
        private IHost? _host;
        private readonly AssemblyName _assemblyName;
        private readonly IMessageSink _diagnosticMessageSink;

        public HostManager(AssemblyName assemblyName, IMessageSink diagnosticMessageSink)
        {
            _assemblyName = assemblyName;
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public IHost? BuildDefaultHost()
        {
            _defaultStartupType = StartupLoader.GetStartupType(_assemblyName);

            _host = StartupLoader.CreateHost(_defaultStartupType, _assemblyName, _diagnosticMessageSink);

            if (_host != null) _hosts.Add(_host);

            return _host;
        }

        public IHost? GetHost(Type? type)
        {
            if (type == null) return _host;

            var startupType = FindStartup(type, out var shared);
            if (startupType == null) return _host;

            if (!shared)
            {
                var host = StartupLoader.CreateHost(startupType, _assemblyName, _diagnosticMessageSink);

                _hosts.Add(host);

                return host;
            }

            if (_hostMap.TryGetValue(startupType, out var startup)) return startup;

            if (startupType == _defaultStartupType) return _hostMap[startupType] = _host!;

            var h = StartupLoader.CreateHost(startupType, _assemblyName, _diagnosticMessageSink);

            _hosts.Add(h);

            return _hostMap[startupType] = h;
        }

        private static Type? FindStartup(Type testClassType, out bool shared)
        {
            shared = true;

            var attr = testClassType.GetCustomAttribute<StartupAttribute>();
            if (attr != null)
            {
                shared = attr.Shared;

                return attr.StartupType;
            }

            var type = testClassType;
            while (type != null)
            {
                var startupType = type.GetNestedType("Startup");
                if (startupType != null) return startupType;

                testClassType = type;

                type = type.DeclaringType;
            }

            var ns = testClassType.Namespace;
            while (true)
            {
                var startupTypeString = "Startup";
                if (!string.IsNullOrEmpty(ns))
                    startupTypeString = ns + ".Startup";

                var startupType = testClassType.Assembly.GetType(startupTypeString);
                if (startupType != null) return startupType;

                var index = ns?.LastIndexOf('.');
                if (index > 0) ns = ns!.Substring(0, index.Value);
                else break;
            }

            return null;
        }

        public Task StartAsync(CancellationToken cancellationToken) =>
            Task.WhenAll(_hosts.Select(x => x.StartAsync(cancellationToken)));

        public Task StopAsync(CancellationToken cancellationToken)
        {
            var tasks = new Task[_hosts.Count];

            for (var index = _hosts.Count - 1; index >= 0; index--)
                tasks[index] = _hosts[index].StopAsync(cancellationToken);

            return Task.WhenAll(tasks);
        }

        public void Dispose()
        {
            for (var index = _hosts.Count - 1; index >= 0; index--)
                _hosts[index].Dispose();
        }
    }
}