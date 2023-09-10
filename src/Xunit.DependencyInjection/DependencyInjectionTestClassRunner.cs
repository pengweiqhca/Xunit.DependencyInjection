namespace Xunit.DependencyInjection;

public class DependencyInjectionTestClassRunner : XunitTestClassRunner
{
    private readonly IServiceProvider _provider;
    private readonly IDictionary<Type, object> _collectionFixtureMappings;
    private AsyncServiceScope? _serviceScope;

    public DependencyInjectionTestClassRunner(IServiceProvider provider,
        ITestClass testClass,
        IReflectionTypeInfo @class,
        IEnumerable<IXunitTestCase> testCases,
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        ITestCaseOrderer testCaseOrderer,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        IDictionary<Type, object> collectionFixtureMappings)
        : base(testClass, @class, testCases, diagnosticMessageSink,
            messageBus, testCaseOrderer, aggregator,
            cancellationTokenSource, collectionFixtureMappings)
    {
        _provider = provider;
        _collectionFixtureMappings = collectionFixtureMappings;
    }

    /// <inheritdoc />
    protected override object?[] CreateTestClassConstructorArguments()
    {
        _provider.GetRequiredService<ITestOutputHelperAccessor>().Output = new TestOutputHelper();

        if ((!Class.Type.GetTypeInfo().IsAbstract ? 0 : Class.Type.GetTypeInfo().IsSealed ? 1 : 0) != 0)
            return Array.Empty<object?>();

        var constructor = SelectTestClassConstructor();

        if (constructor == null) return Array.Empty<object?>();

        var parameters = constructor.GetParameters();

        var objArray = new object?[parameters.Length];
        for (var index = 0; index < parameters.Length; ++index)
        {
            var parameterInfo = parameters[index];
            if (TryGetConstructorArgument(constructor, index, parameterInfo, out var argumentValue))
                objArray[index] = argumentValue;
            else
                objArray[index] = new ArgumentsHelper.DelayArgument(parameterInfo,
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
            argumentValue = ArgumentsHelper.TestOutputHelperArgument.Instance;
            return true;
        }

        if (parameter.ParameterType == typeof(CancellationToken))
        {
            argumentValue = CancellationTokenSource.Token;
            return true;
        }

        return base.TryGetConstructorArgument(constructor, index, parameter, out argumentValue);
    }

    /// <inheritdoc />
    protected override void CreateClassFixture(Type fixtureType)
    {
        var serviceScope = _provider.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();

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
            if (_collectionFixtureMappings.TryGetValue(p.ParameterType, out var arg)) return arg;

            arg = serviceScope.ServiceProvider.GetService(p.ParameterType);

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
        await base.BeforeTestClassFinishedAsync().ConfigureAwait(false);

        foreach (var fixture in ClassFixtureMappings.Values.OfType<IAsyncDisposable>())
            await Aggregator.RunAsync(() => fixture.DisposeAsync().AsTask()).ConfigureAwait(false);

        if (_serviceScope != null) await _serviceScope.DisposeAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod,
        IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, object?[] constructorArguments) =>
        new DependencyInjectionTestMethodRunner(_provider, testMethod, Class, method,
                testCases, DiagnosticMessageSink, MessageBus, new(Aggregator),
                CancellationTokenSource, constructorArguments)
            .RunAsync();
}
