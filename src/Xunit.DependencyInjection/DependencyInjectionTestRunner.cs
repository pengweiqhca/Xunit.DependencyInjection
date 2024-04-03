namespace Xunit.DependencyInjection;

public class DependencyInjectionTestRunner(
    DependencyInjectionContext context,
    ITest test,
    IMessageBus messageBus,
    IReadOnlyDictionary<int, ParameterInfo> fromServices,
    Type testClass,
    object[] constructorArguments,
    MethodInfo testMethod,
    object[] testMethodArguments,
    string skipReason,
    IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource)
    : XunitTestRunner(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments,
        skipReason, beforeAfterAttributes, aggregator, cancellationTokenSource)
{
    /// <inheritdoc />
    protected override async Task<Tuple<decimal, string>> InvokeTestAsync(ExceptionAggregator aggregator)
    {
        var scope = context.RootServices.CreateAsyncScope();

        await using var _ = scope;

        var testOutputHelper = new TestOutputHelper();

        testOutputHelper.Initialize(MessageBus, Test);

        context.RootServices.GetRequiredService<ITestOutputHelperAccessor>().Output = testOutputHelper;

        var raw = new Dictionary<int, object>(TestMethodArguments.Length);
        foreach (var kv in fromServices)
        {
            raw[kv.Key] = TestMethodArguments[kv.Key];

            TestMethodArguments[kv.Key] = kv.Value.ParameterType == typeof(ITestOutputHelper)
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
