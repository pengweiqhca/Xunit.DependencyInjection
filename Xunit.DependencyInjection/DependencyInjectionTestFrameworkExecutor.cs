using Microsoft.Extensions.Hosting;
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
        private readonly HostManager _hostManager;

        public DependencyInjectionTestFrameworkExecutor(
            AssemblyName assemblyName,
            ISourceInformationProvider sourceInformationProvider,
            IMessageSink diagnosticMessageSink)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink) =>
            DisposalTracker.Add(_hostManager = new HostManager(assemblyName, diagnosticMessageSink));

        /// <inheritdoc />
        protected override async void RunTestCases(
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions)
        {
            var exceptions = new List<Exception>();
            IHost? host = null;

            try
            {
                host = _hostManager.BuildDefaultHost();
            }
            catch (TargetInvocationException tie)
            {
                exceptions.Add(tie.InnerException);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            var hostMap = testCases
                .GroupBy(tc => tc.TestMethod.TestClass, TestClassComparer.Instance)
                .ToDictionary(group => group.Key, group =>
            {
                try
                {
                    return _hostManager.GetHost(group.Key.Class.ToRuntimeType());
                }
                catch (TargetInvocationException tie)
                {
                    exceptions.Add(tie.InnerException);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

                return null;
            });

            try
            {
                await _hostManager.StartAsync(default).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            using var runner = new DependencyInjectionTestAssemblyRunner(host?.Services, TestAssembly,
                testCases, hostMap, DiagnosticMessageSink, executionMessageSink, executionOptions, exceptions);

            await runner.RunAsync().ConfigureAwait(false);

            await _hostManager.StopAsync(default).ConfigureAwait(false);
        }
    }
}
