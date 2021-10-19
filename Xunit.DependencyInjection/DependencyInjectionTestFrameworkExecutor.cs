using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestFrameworkExecutor : XunitTestFrameworkExecutor
    {
        private readonly Exception? _exception;
        private readonly HostData _hostData;

        public DependencyInjectionTestFrameworkExecutor(HostData hostData,
            Exception? exception,
            AssemblyName assemblyName,
            ISourceInformationProvider sourceInformationProvider,
            IMessageSink diagnosticMessageSink)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
        {
            _hostData = hostData;
            _exception = exception;

            if (_hostData.AssemblyStartupHost is not null)
            {
                DisposalTracker.Add(_hostData.AssemblyStartupHost);
            }

            foreach (var hostAndModule in _hostData.HostsAndModules)
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
            if (_hostData.AssemblyStartupHost is null && _hostData.HostsAndModules.Length == 0)
            {
                using var runner = new DependencyInjectionTestAssemblyRunner(null, TestAssembly,
                    testCases, DiagnosticMessageSink, executionMessageSink, executionOptions, _exception);

                await runner.RunAsync().ConfigureAwait(false);
            }
            else
            {
                var testCasesGroups = Enumerable.Range(0, _hostData.HostsAndModules.Length + 1).Select(_ => new List<IXunitTestCase>()).ToArray();

                int FindIndexOfTypeInTestCaseGroup(Type type)
                {
                    if (!type.IsNested) return 0;
                    for (int i = 0; i < _hostData.HostsAndModules.Length; i++)
                    {
                        if (type.DeclaringType == _hostData.HostsAndModules[i].ModuleType)
                            return i + 1;
                    }

                    return 0;
                }

                foreach (var testCase in testCases)
                {
                    var declaringType = testCase.Method.ToRuntimeMethod().DeclaringType;
                    var groupIndex = FindIndexOfTypeInTestCaseGroup(declaringType);
                    testCasesGroups[groupIndex].Add(testCase);
                }

                var hostsAndTestCases = new HostAndTestCase[testCasesGroups.Length];
                hostsAndTestCases[0] = new HostAndTestCase(_hostData.AssemblyStartupHost, testCasesGroups[0]);

                for (int i = 1; i < testCasesGroups.Length; i++)
                {
                    hostsAndTestCases[i] = new HostAndTestCase(_hostData.HostsAndModules[i - 1].Host, testCasesGroups[i]);
                }

                var tasks = new Task[testCasesGroups.Length];

                tasks[0] = RunTestCasesWithModuleScope(_hostData.AssemblyStartupHost, testCasesGroups[0]);

                for (int i = 1; i < testCasesGroups.Length; i++)
                {
                    tasks[i] = RunTestCasesWithModuleScope(_hostData.HostsAndModules[i - 1].Host, testCasesGroups[i]);
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);

                async Task RunTestCasesWithModuleScope(HostAndTestCase[] hostAndTestCases)
                {
                    List<Exception> hostExceptions = new();

                    foreach (var hostAndTestCase in hostAndTestCases)
                    {
                        try
                        {
                            if (hostAndTestCase.Host is not null)
                                await hostAndTestCase.Host.StartAsync().ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            hostExceptions.Add(e);
                        }
                    }

                    Exception? ex = hostExceptions.Count == 0 ? null : new AggregateException(hostExceptions);

                    using var runner = new DependencyInjectionTestAssemblyRunner(host.Services, TestAssembly,
                        testCasesByHost, DiagnosticMessageSink, executionMessageSink, executionOptions, _exception, ex);

                    await runner.RunAsync().ConfigureAwait(false);


                    foreach (var hostAndTestCase in hostAndTestCases)
                    {
                        try
                        {
                            if (hostAndTestCase.Host is not null)
                                await hostAndTestCase.Host.StopAsync().ConfigureAwait(false);
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
}
