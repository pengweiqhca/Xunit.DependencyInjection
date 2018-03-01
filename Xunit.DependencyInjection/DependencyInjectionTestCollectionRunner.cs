using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestCollectionRunner : XunitTestCollectionRunner
    {
        private readonly IServiceProvider _provider;
        private readonly IMessageSink _diagnosticMessageSink;

        public DependencyInjectionTestCollectionRunner(IServiceProvider provider,
            ITestCollection testCollection,
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            ITestCaseOrderer testCaseOrderer,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
            : base(testCollection, testCases, diagnosticMessageSink,
                  messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
        {
            _provider = provider;
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        protected override async Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class,
            IEnumerable<IXunitTestCase> testCases)
        {
            using (var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                return await new DependencyInjectionTestClassRunner(scope.ServiceProvider, testClass, @class, testCases,
                        _diagnosticMessageSink, MessageBus, TestCaseOrderer,
                        new ExceptionAggregator(Aggregator), CancellationTokenSource, CollectionFixtureMappings)
                    .RunAsync();
        }
    }
}
