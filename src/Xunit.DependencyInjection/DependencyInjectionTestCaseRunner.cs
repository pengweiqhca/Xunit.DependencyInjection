namespace Xunit.DependencyInjection;

public class DependencyInjectionTestCaseRunner : XunitTestCaseRunner
{
    private readonly DependencyInjectionContext _context;

    public DependencyInjectionTestCaseRunner(DependencyInjectionContext context,
        IXunitTestCase testCase,
        string displayName,
        string skipReason,
        object?[] constructorArguments,
        object[] testMethodArguments,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
        : base(testCase, displayName, skipReason, constructorArguments, testMethodArguments, messageBus,
            aggregator, cancellationTokenSource) => _context = context;

    /// <inheritdoc />
    protected override Task<RunSummary> RunTestAsync() =>
        new DependencyInjectionTestRunner(_context, new XunitTest(TestCase, DisplayName), MessageBus,
                FromServicesAttribute.CreateFromServices(TestMethod),
                TestClass, ConstructorArguments, TestMethod, TestMethodArguments, SkipReason,
                BeforeAfterAttributes, new(Aggregator), CancellationTokenSource)
            .RunAsync();
}
