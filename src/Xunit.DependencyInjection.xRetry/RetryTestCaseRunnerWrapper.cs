using xRetry.v3;
using Xunit.Sdk;
using Xunit.v3;

#pragma warning disable IDE1006 // Naming Styles
namespace Xunit.DependencyInjection.xRetry;
#pragma warning restore IDE1006 // Naming Styles

public class RetryTestCaseRunnerWrapper : DependencyInjectionTestCaseRunnerWrapper
{
    /// <inheritdoc />
    public override Type TestCaseType => typeof(RetryTestCase);

    /// <inheritdoc />
    public override ValueTask<RunSummary> RunAsync(DependencyInjectionContext context, IXunitTestCase testCase,
        IReadOnlyCollection<IXunitTest> tests,
        IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource,
        string displayName, string? skipReason, ExplicitOption explicitOption, object?[] constructorArguments)
    {
        if (testCase is not IRetryableTestCase retryableTestCase)
            throw new ArgumentException("Must be a retryable test case", nameof(testCase));

        return RetryTestCaseRunner.Run(retryableTestCase, messageBus, cancellationTokenSource,
            blockingMessageBus => base.RunAsync(context, testCase, tests, blockingMessageBus, aggregator,
                cancellationTokenSource, displayName, skipReason, explicitOption, constructorArguments));
    }
}
