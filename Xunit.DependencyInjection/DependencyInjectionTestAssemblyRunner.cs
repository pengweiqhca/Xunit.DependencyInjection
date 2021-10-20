using Microsoft.Extensions.Hosting;
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
        private readonly IReadOnlyDictionary<ITestClass, IHost?> _hostMap;

        public DependencyInjectionTestAssemblyRunner(IServiceProvider? provider,
            ITestAssembly testAssembly,
            IEnumerable<IXunitTestCase> testCases,
            IReadOnlyDictionary<ITestClass, IHost?> hostMap,
            IMessageSink diagnosticMessageSink,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions,
            IEnumerable<Exception> exceptions)
            : base(testAssembly, testCases, diagnosticMessageSink,
                executionMessageSink, executionOptions)
        {
            _provider = provider;
            _hostMap = hostMap;

            foreach (var exception in exceptions) Aggregator.Add(exception);
        }

        /// <inheritdoc />
        protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus,
            ITestCollection testCollection,
            IEnumerable<IXunitTestCase> testCases,
            CancellationTokenSource cancellationTokenSource)
        {
            if (_provider == null)
                return base.RunTestCollectionAsync(messageBus, testCollection, testCases, cancellationTokenSource);

            return new DependencyInjectionTestCollectionRunner(_provider, testCollection,
                    testCases, _hostMap, DiagnosticMessageSink, messageBus, TestCaseOrderer,
                    new ExceptionAggregator(Aggregator), cancellationTokenSource)
                .RunAsync();
        }
    }
}
