using xRetry;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection.xRetry;

public class RetryTestCaseRunnerWrapper : DependencyInjectionTestCaseRunnerWrapper
{
    /// <inheritdoc />
    public override Type TestCaseType => typeof(RetryTestCase);

    /// <inheritdoc />
    public override Task<RunSummary> RunAsync(IXunitTestCase testCase, DependencyInjectionContext context,
        IMessageSink diagnosticMessageSink, IMessageBus messageBus, object?[] constructorArguments,
        ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
    {
        if (testCase is not IRetryableTestCase retryableTestCase)
            throw new ArgumentException("Must be a retryable test case", nameof(testCase));

        return RetryTestCaseRunner.RunAsync(retryableTestCase, diagnosticMessageSink, messageBus,
            cancellationTokenSource,
            blockingMessageBus => base.RunAsync(retryableTestCase, context, diagnosticMessageSink, blockingMessageBus,
                constructorArguments, aggregator, cancellationTokenSource));
    }
}
