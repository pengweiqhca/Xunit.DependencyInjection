using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestCollectionRunner : XunitTestCollectionRunner
    {
        private readonly IServiceProvider _provider;
        private IServiceScope? _serviceScope;
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

        /// <inheritdoc />
        protected override void CreateCollectionFixture(Type fixtureType)
        {
            _serviceScope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            Aggregator.Run(() => CollectionFixtureMappings[fixtureType] =
                ActivatorUtilities.GetServiceOrCreateInstance(_serviceScope.ServiceProvider, fixtureType));
        }

        /// <inheritdoc/>
        protected override async Task BeforeTestCollectionFinishedAsync()
        {
            await base.BeforeTestCollectionFinishedAsync();

             _serviceScope?.Dispose();
        }

        /// <inheritdoc />
        protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass,
            IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases) =>
            new DependencyInjectionTestClassRunner(_provider, testClass, @class, testCases,
                    _diagnosticMessageSink, MessageBus, TestCaseOrderer,
                    new ExceptionAggregator(Aggregator), CancellationTokenSource, CollectionFixtureMappings)
                .RunAsync();
    }
}
