namespace Xunit.DependencyInjection;

public class DependencyInjectionTestClassRunner(
    DependencyInjectionTestContext context,
    ITestClass testClass,
    IReflectionTypeInfo @class,
    IEnumerable<IXunitTestCase> testCases,
    IMessageSink diagnosticMessageSink,
    IMessageBus messageBus,
    ITestCaseOrderer testCaseOrderer,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource,
    IDictionary<Type, object> collectionFixtureMappings)
    : XunitTestClassRunner(testClass, @class, testCases, diagnosticMessageSink,
        messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings)
{
    private AsyncServiceScope? _serviceScope;

    private IDictionary<Type, object> CollectionFixtureMappings { get; } = collectionFixtureMappings;

    /// <inheritdoc />
    protected override object?[] CreateTestClassConstructorArguments()
    {
        if ((!Class.Type.GetTypeInfo().IsAbstract ? 0 : Class.Type.GetTypeInfo().IsSealed ? 1 : 0) != 0)
            return [];

        var constructor = SelectTestClassConstructor();

        if (constructor == null) return [];

        var parameters = constructor.GetParameters();

        var objArray = new object?[parameters.Length];
        for (var index = 0; index < parameters.Length; ++index)
        {
            var parameterInfo = parameters[index];

            objArray[index] = TryGetConstructorArgument(constructor, index, parameterInfo, out var argumentValue)
                ? argumentValue
                : new TestHelper.DelayArgument(parameterInfo,
                    unusedArguments => FormatConstructorArgsMissingMessage(constructor, unusedArguments));
        }

        return objArray;
    }

    /// <inheritdoc />
    protected override bool TryGetConstructorArgument(ConstructorInfo constructor, int index, ParameterInfo parameter,
        out object? argumentValue)
    {
        if (parameter.ParameterType == typeof(ITestOutputHelper))
        {
            argumentValue = TestHelper.TestOutputHelperArgument.Instance;
            return true;
        }

        if (parameter.ParameterType != typeof(CancellationToken))
            return base.TryGetConstructorArgument(constructor, index, parameter, out argumentValue);

        argumentValue = CancellationTokenSource.Token;

        return true;
    }

    /// <inheritdoc />
    protected override void CreateClassFixture(Type fixtureType)
    {
        var serviceScope = context.RootServices.CreateAsyncScope();

        _serviceScope = serviceScope;

        var ctors = fixtureType.GetTypeInfo()
            .DeclaredConstructors
            .Where(ci => !ci.IsStatic && ci.IsPublic)
            .ToList();

        if (ctors.Count != 1)
        {
            Aggregator.Add(new TestClassException($"Class fixture type '{fixtureType.FullName}' may only define a single public constructor."));
            return;
        }

        var missingParameters = new List<ParameterInfo>();
        var ctorArgs = ctors[0].GetParameters().Select(p =>
        {
            if (CollectionFixtureMappings.TryGetValue(p.ParameterType, out var arg)) return arg;

            arg = serviceScope.ServiceProvider.GetService(p);

            if (arg == null) missingParameters.Add(p);

            return arg;
        }).ToArray();

        if (missingParameters.Count > 0)
            Aggregator.Add(new TestClassException(
                $"Class fixture type '{fixtureType.FullName}' had one or more unresolved constructor arguments: {string.Join(", ", missingParameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))}"));
        else Aggregator.Run(() => ClassFixtureMappings[fixtureType] = ctors[0].Invoke(ctorArgs));
    }

    /// <inheritdoc />
    protected override async Task BeforeTestClassFinishedAsync()
    {
        await base.BeforeTestClassFinishedAsync();

        foreach (var fixture in ClassFixtureMappings.Values.OfType<IAsyncDisposable>())
            await Aggregator.RunAsync(() => fixture.DisposeAsync().AsTask());

        if (_serviceScope is { } disposable) await disposable.DisposeAsync();
    }

    // This method has been slightly modified from the original implementation to run tests in parallel
    // https://github.com/xunit/xunit/blob/2.4.2/src/xunit.execution/Sdk/Frameworks/Runners/TestClassRunner.cs#L194-L219
    protected override async Task<RunSummary> RunTestMethodsAsync()
    {
        if (context.DisableParallelization ||
            TestCases.Count() < 2 ||
            TestClass.Class.GetCustomAttributes(typeof(CollectionDefinitionAttribute)).FirstOrDefault() is { } attr &&
            attr.GetNamedArgument<bool>(nameof(CollectionDefinitionAttribute.DisableParallelization)) ||
            TestClass.Class.GetCustomAttributes(typeof(DisableParallelizationAttribute)).Any() ||
            TestClass.Class.GetCustomAttributes(typeof(CollectionAttribute)).Any() && !context.ForcedParallelization)
            return await base.RunTestMethodsAsync();

        IEnumerable<IXunitTestCase> orderedTestCases;
        try
        {
            orderedTestCases = TestCaseOrderer.OrderTestCases(TestCases);
        }
        catch (Exception ex)
        {
            ex = ex.Unwrap();

            DiagnosticMessageSink.OnMessage(new DiagnosticMessage(
                $"Test case orderer '{TestCaseOrderer.GetType().FullName}' threw '{ex.GetType().FullName}' during ordering: {ex.Message}{Environment.NewLine}{ex.StackTrace}"));

            orderedTestCases = TestCases.ToList();
        }

        var constructorArguments = CreateTestClassConstructorArguments();

        var methodTasks = orderedTestCases.GroupBy(tc => tc.TestMethod, TestMethodComparer.Instance)
            .Select(m => RunTestMethodAsync(m.Key, (IReflectionMethodInfo)m.Key.Method, m, constructorArguments));

        var summary = new RunSummary();

        foreach (var methodSummary in await Task.WhenAll(methodTasks))
            summary.Aggregate(methodSummary);

        return summary;
    }

    /// <inheritdoc />
    protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod,
        IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, object?[] constructorArguments) =>
        new DependencyInjectionTestMethodRunner(context, testMethod, Class, method,
                testCases, DiagnosticMessageSink, MessageBus, new(Aggregator),
                CancellationTokenSource, constructorArguments)
            .RunAsync();
}
