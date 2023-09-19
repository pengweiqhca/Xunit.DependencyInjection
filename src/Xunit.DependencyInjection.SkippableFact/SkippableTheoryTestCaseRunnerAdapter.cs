using System.Linq.Expressions;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection.SkippableFact;

public class SkippableTheoryTestCaseRunnerWrapper : DependencyInjectionTheoryTestCaseRunnerWrapper
{
    private static readonly Func<IXunitTestCase, string[]> GetSkippingExceptionNames;

    static SkippableTheoryTestCaseRunnerWrapper()
    {
        var testCaseRunner = Expression.Parameter(typeof(IXunitTestCase));

        GetSkippingExceptionNames = Expression.Lambda<Func<IXunitTestCase, string[]>>(
            Expression.PropertyOrField(Expression.Convert(testCaseRunner, typeof(SkippableTheoryTestCase)),
                "SkippingExceptionNames"), testCaseRunner).Compile();
    }

    /// <inheritdoc />
    public override Type TestCaseType => typeof(SkippableTheoryTestCase);

    /// <inheritdoc />
    public override async Task<RunSummary> RunAsync(IXunitTestCase testCase, DependencyInjectionContext context,
        IMessageSink diagnosticMessageSink, IMessageBus messageBus, object?[] constructorArguments,
        ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
    {
        var messageBusInterceptor = new SkippableTestMessageBus(messageBus, GetSkippingExceptionNames(testCase));

        var result = await base.RunAsync(testCase, context, diagnosticMessageSink,
                messageBusInterceptor, constructorArguments, aggregator, cancellationTokenSource);

        result.Failed -= messageBusInterceptor.SkippedCount;
        result.Skipped += messageBusInterceptor.SkippedCount;

        return result;
    }
}
