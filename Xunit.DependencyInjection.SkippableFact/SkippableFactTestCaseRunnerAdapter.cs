using System.Linq.Expressions;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection.SkippableFact;

public class SkippableFactTestCaseRunnerWrapper : DependencyInjectionTestCaseRunnerWrapper
{
    private static readonly Func<IXunitTestCase, string[]> GetSkippingExceptionNames;

    static SkippableFactTestCaseRunnerWrapper()
    {
        var testCaseRunner = Expression.Parameter(typeof(IXunitTestCase));

        GetSkippingExceptionNames = Expression.Lambda<Func<IXunitTestCase, string[]>>(
            Expression.PropertyOrField(Expression.Convert(testCaseRunner, typeof(SkippableFactTestCase)),
                "SkippingExceptionNames"), testCaseRunner).Compile();
    }

    public override Type TestCaseType => typeof(SkippableFactTestCase);

    public override async Task<RunSummary> RunAsync(IXunitTestCase testCase, IServiceProvider provider, IMessageSink diagnosticMessageSink, IMessageBus messageBus, object?[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
    {
        var messageBusInterceptor = new SkippableTestMessageBus(messageBus, GetSkippingExceptionNames(testCase));

        var result = await base.RunAsync(testCase, provider, diagnosticMessageSink,
                messageBusInterceptor, constructorArguments, aggregator, cancellationTokenSource)
            .ConfigureAwait(false);

        result.Failed -= messageBusInterceptor.SkippedCount;
        result.Skipped += messageBusInterceptor.SkippedCount;

        return result;
    }
}