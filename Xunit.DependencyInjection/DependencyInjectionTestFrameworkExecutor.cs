using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestFrameworkExecutor : XunitTestFrameworkExecutor
    {
        private readonly Exception? _exception;
        private readonly HostFinder _hostFinder;

        public DependencyInjectionTestFrameworkExecutor(HostFinder hostFinder,
            Exception? exception,
            AssemblyName assemblyName,
            ISourceInformationProvider sourceInformationProvider,
            IMessageSink diagnosticMessageSink)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
        {
            _hostFinder = hostFinder;
            _exception = exception;

            if (_hostFinder.AssemblyStartupHost is not null)
            {
                DisposalTracker.Add(_hostFinder.AssemblyStartupHost);
            }

            foreach (var hostAndModule in _hostFinder.HostsAndModules)
            {
                DisposalTracker.Add(hostAndModule.Host);
            }
        }

        /// <inheritdoc />
        protected override async void RunTestCases(
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions)
        {
            if (_hostFinder.AssemblyStartupHost is null && _hostFinder.HostsAndModules.Length == 0)
            {
                using var runner = new DependencyInjectionTestAssemblyRunner(_hostFinder, testCases, TestAssembly
                  , DiagnosticMessageSink, executionMessageSink, executionOptions, _exception);

                await runner.RunAsync().ConfigureAwait(false);
            }
            else
            {
                List<Exception> hostExceptions = new();

                foreach (var host in _hostFinder.HostsAndModules.Select(x => x.Host).Concat(new[] {_hostFinder.AssemblyStartupHost}))
                {
                    try
                    {
                        if (host is not null)
                            await host.StartAsync().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        hostExceptions.Add(e);
                    }
                }

                Exception? ex = hostExceptions.Count == 0 ? null : new AggregateException(hostExceptions);

                using var runner = new DependencyInjectionTestAssemblyRunner(_hostFinder, testCases, TestAssembly,
                    DiagnosticMessageSink, executionMessageSink, executionOptions, _exception, ex);

                await runner.RunAsync().ConfigureAwait(false);


                foreach (var host in _hostFinder.HostsAndModules.Select(x => x.Host).Concat(new[] {_hostFinder.AssemblyStartupHost}))
                {
                    try
                    {
                        if (host is not null)
                            await host.StopAsync().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        hostExceptions.Add(e);
                    }
                }

                if (hostExceptions.Count > 0)
                    throw new AggregateException(hostExceptions);
            }
        }
    }
}
