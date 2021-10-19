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
        private readonly HostFinder _hostFinder;
        private readonly HashSet<IServiceScope> _serviceScopes = new();
        private readonly IMessageSink _diagnosticMessageSink;

        public DependencyInjectionTestCollectionRunner(HostFinder hostFinder,
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
            _hostFinder = hostFinder;
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        /// <inheritdoc />
        protected override void CreateCollectionFixture(Type fixtureType)
        {
            var host = _hostFinder.GetHostForTestFixture(fixtureType);
            if (host is not null)
            {
                var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

                _serviceScopes.Add(serviceScope);

                Aggregator.Run(() => CollectionFixtureMappings[fixtureType] =
                    ActivatorUtilities.GetServiceOrCreateInstance(serviceScope.ServiceProvider, fixtureType));
            }
        }

        /// <inheritdoc/>
        protected override async Task BeforeTestCollectionFinishedAsync()
        {
            await base.BeforeTestCollectionFinishedAsync().ConfigureAwait(false);

            foreach (var serviceScope in _serviceScopes)
                serviceScope.Dispose();
            _serviceScopes.Clear();
        }

        /// <inheritdoc />
        protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass,
            IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases) =>
            new DependencyInjectionTestClassRunner(_hostFinder, testClass, @class, testCases,
                    _diagnosticMessageSink, MessageBus, TestCaseOrderer,
                    new ExceptionAggregator(Aggregator), CancellationTokenSource, CollectionFixtureMappings)
                .RunAsync();
    }
}
