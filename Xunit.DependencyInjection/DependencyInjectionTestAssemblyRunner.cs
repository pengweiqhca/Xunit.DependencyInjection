using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestAssemblyRunner : XunitTestAssemblyRunner
    {
        private readonly IServiceProvider? _provider;

        public DependencyInjectionTestAssemblyRunner(IServiceProvider? provider,
            Exception? exception,
            ITestAssembly testAssembly,
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink diagnosticMessageSink,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions)
            : base(testAssembly, testCases, diagnosticMessageSink,
                executionMessageSink, executionOptions)
        {
            _provider = provider;

            if (exception != null) Aggregator.Add(exception);
        }

        protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus,
            ITestCollection testCollection,
            IEnumerable<IXunitTestCase> testCases,
            CancellationTokenSource cancellationTokenSource)
        {
            if (_provider == null)
                return base.RunTestCollectionAsync(messageBus, testCollection, testCases, cancellationTokenSource);

            return new DependencyInjectionTestCollectionRunner(_provider, testCollection,
                    testCases, DiagnosticMessageSink, messageBus, TestCaseOrderer,
                    new ExceptionAggregator(Aggregator), cancellationTokenSource)
                .RunAsync();
        }
    }
}
