namespace Xunit.DependencyInjection;

public class DependencyInjectionTestRunner : XunitTestRunner
{
    private readonly DependencyInjectionContext _context;
    private readonly IReadOnlyDictionary<int, Type> _fromServices;

    public DependencyInjectionTestRunner(DependencyInjectionContext context,
        ITest test,
        IMessageBus messageBus,
        IReadOnlyDictionary<int, Type> fromServices,
        Type testClass, object[] constructorArguments,
        MethodInfo testMethod,
        object[] testMethodArguments,
        string skipReason,
        IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
        : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments,
            skipReason, beforeAfterAttributes, aggregator, cancellationTokenSource)
    {
        _context = context;
        _fromServices = fromServices;
    }

    /// <inheritdoc />
    protected override async Task<Tuple<decimal, string>> InvokeTestAsync(ExceptionAggregator aggregator)
    {
        var scope = _context.RootServices.CreateAsyncScope();

        await using var _ = scope;

        var testOutputHelper = new TestOutputHelper();

        testOutputHelper.Initialize(MessageBus, Test);

        _context.RootServices.GetRequiredService<ITestOutputHelperAccessor>().Output = testOutputHelper;

        var raw = new Dictionary<int, object>(TestMethodArguments.Length);
        foreach (var kv in _fromServices)
        {
            raw[kv.Key] = TestMethodArguments[kv.Key];

            TestMethodArguments[kv.Key] = kv.Value == typeof(ITestOutputHelper)
                ? testOutputHelper
                : scope.ServiceProvider.GetService(kv.Value);
        }

        var item = await new DependencyInjectionTestInvoker(scope.ServiceProvider, Test, MessageBus, TestClass,
                scope.ServiceProvider.CreateTestClassConstructorArguments(ConstructorArguments, aggregator),
                TestMethod, TestMethodArguments, BeforeAfterAttributes, aggregator, CancellationTokenSource)
            .RunAsync();

        foreach (var kv in raw)
            TestMethodArguments[kv.Key] = kv.Value;

        var output = testOutputHelper.Output;

        testOutputHelper.Uninitialize();

        return Tuple.Create(item, output);
    }
}
