using System.Linq.Expressions;

namespace Xunit.DependencyInjection;

public class DependencyInjectionTheoryTestCaseRunner(
    DependencyInjectionContext context,
    IXunitTestCase testCase,
    string displayName,
    string skipReason,
    object?[] constructorArguments,
    IMessageSink diagnosticMessageSink,
    IMessageBus messageBus,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource)
    : XunitTheoryTestCaseRunner(testCase, displayName, skipReason, constructorArguments, diagnosticMessageSink,
        messageBus, aggregator,
        cancellationTokenSource)
{
    private static readonly Func<XunitTheoryTestCaseRunner, List<XunitTestRunner>> GetTestRunners;
    private static readonly Func<TestRunner<IXunitTestCase>, ITest> GetTest;
    private static readonly Func<TestRunner<IXunitTestCase>, object[]> GetTestMethodArguments;

    static DependencyInjectionTheoryTestCaseRunner()
    {
        var testCaseRunner = Expression.Parameter(typeof(XunitTheoryTestCaseRunner));

        GetTestRunners = Expression.Lambda<Func<XunitTheoryTestCaseRunner, List<XunitTestRunner>>>(Expression.PropertyOrField(testCaseRunner, "testRunners"), testCaseRunner).Compile();

        var testRunner = Expression.Parameter(typeof(TestRunner<IXunitTestCase>));

        GetTest = Expression.Lambda<Func<TestRunner<IXunitTestCase>, ITest>>(Expression.PropertyOrField(testRunner, "Test"), testRunner).Compile();
        GetTestMethodArguments = Expression.Lambda<Func<TestRunner<IXunitTestCase>, object[]>>(Expression.PropertyOrField(testRunner, "TestMethodArguments"), testRunner).Compile();
    }

    /// <inheritdoc />
    protected override async Task AfterTestCaseStartingAsync()
    {
        await base.AfterTestCaseStartingAsync();

        var fromServices = FromServicesAttribute.CreateFromServices(TestMethod);
        var runners = GetTestRunners(this);
        for (var index = 0; index < runners.Count; index++)
            if (runners[index] is TestRunner<IXunitTestCase> runner)
                runners[index] = new DependencyInjectionTestRunner(context, GetTest(runner), MessageBus,
                    fromServices, TestClass, index == 0 ? ConstructorArguments : [..ConstructorArguments],
                    TestMethod, GetTestMethodArguments(runner),
                    SkipReason, BeforeAfterAttributes, Aggregator, CancellationTokenSource);
    }

    public new async Task<RunSummary> RunAsync()
    {
        await using (TheoryTestCaseDataContext.BeginContext(context.RootServices))
            return await base.RunAsync();
    }
}
