using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestAssemblyRunner : XunitTestAssemblyRunner
    {
        private readonly HostFinder _hostFinder;

        public DependencyInjectionTestAssemblyRunner(HostFinder hostFinder,
            IEnumerable<IXunitTestCase> testCases,
            ITestAssembly testAssembly,
            IMessageSink diagnosticMessageSink,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions,
            params Exception?[] exceptions)
            : base(testAssembly, testCases, diagnosticMessageSink,
                executionMessageSink, executionOptions)
        {
            _hostFinder = hostFinder;

            foreach (var exception in exceptions) if (exception != null) Aggregator.Add(exception);
        }

        /// <inheritdoc />
        protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus,
            ITestCollection testCollection,
            IEnumerable<IXunitTestCase> testCases,
            CancellationTokenSource cancellationTokenSource)
        {
            if (_hostFinder.AssemblyStartupHost is null && _hostFinder.HostsAndModules.Length == 0)
                return base.RunTestCollectionAsync(messageBus, testCollection, testCases, cancellationTokenSource);

            return new DependencyInjectionTestCollectionRunner(_hostFinder, testCollection, testCases, DiagnosticMessageSink, messageBus, TestCaseOrderer,
                    new ExceptionAggregator(Aggregator), cancellationTokenSource)
                .RunAsync();
        }
    }
}
