using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestCaseRunnerWrapper : IXunitTestCaseRunnerWrapper
    {
        /// <inheritdoc />
        public virtual Type TestCaseType => typeof(XunitTestCase);

        /// <inheritdoc />
        public virtual Task<RunSummary> RunAsync(IXunitTestCase testCase,
            IServiceProvider provider,
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object?[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource) =>
            new DependencyInjectionTestCaseRunner(provider, testCase, testCase.DisplayName,
                testCase.SkipReason, constructorArguments, testCase.TestMethodArguments,
                messageBus, aggregator, cancellationTokenSource).RunAsync();
    }
}
