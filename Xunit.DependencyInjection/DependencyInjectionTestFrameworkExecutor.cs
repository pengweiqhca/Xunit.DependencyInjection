using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestFrameworkExecutor : XunitTestFrameworkExecutor
    {
        private readonly IHost _host;

        public DependencyInjectionTestFrameworkExecutor(IHost host,
            AssemblyName assemblyName,
            ISourceInformationProvider sourceInformationProvider,
            IMessageSink diagnosticMessageSink)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
        {
            _host = host;

            DisposalTracker.Add(_host);
        }

        protected override async void RunTestCases(
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions)
        {
            await _host.StartAsync();

            using (var runner = new DependencyInjectionTestAssemblyRunner(_host.Services, TestAssembly,
                testCases, DiagnosticMessageSink, executionMessageSink, executionOptions))
                await runner.RunAsync();

            await _host.StopAsync();
        }
    }
}
