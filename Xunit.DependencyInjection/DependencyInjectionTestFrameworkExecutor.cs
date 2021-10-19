using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestFrameworkExecutor : XunitTestFrameworkExecutor
    {
        private readonly Exception? _exception;
        private readonly HostAndModule[] _hostAndModules;

        public DependencyInjectionTestFrameworkExecutor(HostAndModule[] hostAndModules,
            Exception? exception,
            AssemblyName assemblyName,
            ISourceInformationProvider sourceInformationProvider,
            IMessageSink diagnosticMessageSink)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
        {
            _hostAndModules = hostAndModules;
            _exception = exception;

            foreach (var hostAndModule in _hostAndModules)
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
            if (_hostAndModules.Length == 0)
            {
                using var runner = new DependencyInjectionTestAssemblyRunner(null, TestAssembly,
                    testCases, DiagnosticMessageSink, executionMessageSink, executionOptions, _exception);

                await runner.RunAsync().ConfigureAwait(false);
            }
            else
            {

                var moduleTestCases = _hostAndModules.Select(_ => new List<IXunitTestCase>());
                var offset = 0;
                if (_hostAndModules[0].ModuleType is not null)
                {
                    offset = 1;
                    moduleTestCases = new[]{ new List<IXunitTestCase>() }.Concat(moduleTestCases);
                }

                var testCasesGroups = moduleTestCases.ToArray();

                int FindIndexOfTypeInTestCaseGroup(Type type)
                {
                    if (!type.IsNested) return 0;
                    for (int i = 0; i < _hostAndModules.Length; i++)
                    {
                        if (type.DeclaringType == _hostAndModules[i].ModuleType)
                            return i + offset;
                    }

                    return 0;
                }

                foreach (var testCase in testCases)
                {
                    var declaringType = testCase.Method.ToRuntimeMethod().DeclaringType;
                    var groupIndex = FindIndexOfTypeInTestCaseGroup(declaringType);
                    testCasesGroups[groupIndex].Add(testCase);
                }

                if (testCasesGroups.Length != _hostAndModules.Length)
                {
                    await RunTestCasesWithModuleScope(null, testCasesGroups[0]).ConfigureAwait(false);
                    for (int i = 1; i < testCasesGroups.Length; i++)
                    {
                        await RunTestCasesWithModuleScope(_hostAndModules[i].Host, testCasesGroups[i]).ConfigureAwait(false);
                    }
                }
                else
                {
                    for (int i = 0; i < testCasesGroups.Length; i++)
                    {
                        await RunTestCasesWithModuleScope(_hostAndModules[i].Host, testCasesGroups[i]).ConfigureAwait(false);
                    }
                }

                async Task RunTestCasesWithModuleScope(IHost? host, List<IXunitTestCase> testCasesByHost)
                {
                    if (host is null)
                    {
                        using var runner = new DependencyInjectionTestAssemblyRunner(null, TestAssembly,
                            testCasesByHost, DiagnosticMessageSink, executionMessageSink, executionOptions, _exception);

                        await runner.RunAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        Exception? ex = null;
                        try
                        {
                            await host.StartAsync().ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            ex = e;
                        }

                        using var runner = new DependencyInjectionTestAssemblyRunner(host.Services, TestAssembly,
                            testCasesByHost, DiagnosticMessageSink, executionMessageSink, executionOptions, _exception, ex);

                        await runner.RunAsync().ConfigureAwait(false);

                        await host.StopAsync().ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
