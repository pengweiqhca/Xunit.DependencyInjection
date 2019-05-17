using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestFrameworkExecutor : XunitTestFrameworkExecutor
    {
        [CanBeNull] private readonly Exception _exception;
        [CanBeNull] private readonly IHost _host;

        public DependencyInjectionTestFrameworkExecutor(IHost host,
            Exception exception,
            AssemblyName assemblyName,
            ISourceInformationProvider sourceInformationProvider,
            IMessageSink diagnosticMessageSink)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
        {
            _host = host;
            _exception = exception;

            if (_host != null) DisposalTracker.Add(_host);
        }

        protected override async void RunTestCases(
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions)
        {
            if (_host == null)
            {
                using (var runner = new DependencyInjectionTestAssemblyRunner(null, _exception, TestAssembly,
                    testCases, DiagnosticMessageSink, executionMessageSink, executionOptions))
                    await runner.RunAsync();

                return;
            }

            await _host.StartAsync();

            using (var runner = new DependencyInjectionTestAssemblyRunner(_host.Services, _exception, TestAssembly,
                testCases, DiagnosticMessageSink, executionMessageSink, executionOptions))
                await runner.RunAsync();

            await _host.StopAsync();
        }
    }
}
