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
        private readonly HostAndTestCase[] _hostsAndTestCases;

        public DependencyInjectionTestAssemblyRunner(HostAndTestCase[] hostsAndTestCases,
            ITestAssembly testAssembly,
            IMessageSink diagnosticMessageSink,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions,
            params Exception?[] exceptions)
            : base(testAssembly, hostsAndTestCases.SelectMany(x => x.TestCases), diagnosticMessageSink,
                executionMessageSink, executionOptions)
        {
            _hostsAndTestCases = hostsAndTestCases;

            foreach (var exception in exceptions) if (exception != null) Aggregator.Add(exception);
        }

        /// <inheritdoc />
        protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus,
            ITestCollection testCollection,
            IEnumerable<IXunitTestCase> testCases,
            CancellationTokenSource cancellationTokenSource)
        {
            if (_hostsAndTestCases.Length == 1 && _hostsAndTestCases[0].Host is null)
                return base.RunTestCollectionAsync(messageBus, testCollection, testCases, cancellationTokenSource);

            return new DependencyInjectionTestCollectionRunner(_hostsAndTestCases, testCollection, DiagnosticMessageSink, messageBus, TestCaseOrderer,
                    new ExceptionAggregator(Aggregator), cancellationTokenSource)
                .RunAsync();
        }
    }
}
