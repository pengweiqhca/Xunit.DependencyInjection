using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestFrameworkExecutor : XunitTestFrameworkExecutor
    {
        private readonly IServiceProvider _provider;

        public DependencyInjectionTestFrameworkExecutor(IServiceProvider provider,
            AssemblyName assemblyName,
            ISourceInformationProvider sourceInformationProvider,
            IMessageSink diagnosticMessageSink)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
        {
            _provider = provider;

            var scope = _provider.GetService<IServiceScope>();
            if (scope != null)
                DisposalTracker.Add(scope);
        }

        protected override async void RunTestCases(
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions)
        {
            using (var runner = new DependencyInjectionTestAssemblyRunner(_provider, TestAssembly,
                testCases, DiagnosticMessageSink, executionMessageSink, executionOptions))
                await runner.RunAsync();
        }
    }
}
