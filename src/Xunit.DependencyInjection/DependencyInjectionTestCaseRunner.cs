namespace Xunit.DependencyInjection;

public class DependencyInjectionTestCaseRunner(
    DependencyInjectionContext context,
    IXunitTestCase testCase,
    string displayName,
    string skipReason,
    object?[] constructorArguments,
    object[] testMethodArguments,
    IMessageBus messageBus,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource)
    : XunitTestCaseRunner(testCase, displayName, skipReason, constructorArguments, testMethodArguments, messageBus,
        aggregator, cancellationTokenSource)
{
    /// <inheritdoc />
    protected override Task<RunSummary> RunTestAsync() =>
        new DependencyInjectionTestRunner(context, new XunitTest(TestCase, DisplayName), MessageBus,
                FromServicesAttribute.CreateFromServices(TestMethod),
                TestClass, ConstructorArguments, TestMethod, TestMethodArguments, SkipReason,
                BeforeAfterAttributes, new(Aggregator), CancellationTokenSource)
            .RunAsync();
}
