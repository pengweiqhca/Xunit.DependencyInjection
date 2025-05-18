using Xunit.Internal;

namespace Xunit.DependencyInjection;

public class DependencyInjectionTestClassRunner(DependencyInjectionTestContext context)
    : XunitTestClassRunner
{
    private AsyncServiceScope? _serviceScope;

    /// <inheritdoc />
    protected override async ValueTask<object?[]> CreateTestClassConstructorArguments(XunitTestClassRunnerContext ctxt)
    {
        if (ctxt.Aggregator.HasExceptions || SelectTestClassConstructor(ctxt) is not { } constructor) return [];

        var parameters = constructor.GetParameters();

        var objArray = new object?[parameters.Length];
        for (var index = 0; index < parameters.Length; ++index)
        {
            var parameterInfo = parameters[index];

            var argumentValue = await GetConstructorArgument(ctxt, constructor, index, parameterInfo);

            objArray[index] = argumentValue ?? new TestHelper.DelayArgument(parameterInfo,
                unusedArguments => FormatConstructorArgsMissingMessage(ctxt, constructor, unusedArguments));
        }

        return objArray;
    }

    /// <inheritdoc />
    protected override ValueTask<object?> GetConstructorArgument(XunitTestClassRunnerContext ctxt,
        ConstructorInfo constructor, int index,
        ParameterInfo parameter)
    {
        if (parameter.ParameterType == typeof(ITestContextAccessor))
            return new(TestContextAccessor.Instance);

        if (parameter.ParameterType == typeof(ITestOutputHelper))
            return new(TestHelper.TestOutputHelperArgument.Instance);

        return parameter.ParameterType == typeof(CancellationToken)
            ? new(ctxt.CancellationTokenSource.Token)
            : ctxt.ClassFixtureMappings.GetFixture(parameter.ParameterType);
    }

    /// <inheritdoc />
    protected override async ValueTask<bool> OnTestClassStarting(XunitTestClassRunnerContext ctxt)
    {
        if (ctxt.TestClass.ClassFixtureTypes.Count > 0)
        {
            var serviceScope = context.RootServices.CreateAsyncScope();

            _serviceScope = serviceScope;

            await ctxt.ClassFixtureMappings.CreateFixtures(ctxt.TestClass.ClassFixtureTypes, ctxt.Aggregator,
                serviceScope.ServiceProvider);
        }

        return await base.OnTestClassStarting(ctxt);
    }

    protected override async ValueTask<bool> OnTestClassFinished(XunitTestClassRunnerContext ctxt, RunSummary summary)
    {
        if (_serviceScope is not { } disposable)
            return await base.OnTestClassFinished(ctxt, summary);

        try
        {
            ctxt.ClassFixtureMappings.ClearFixtures(ctxt.TestClass.ClassFixtureTypes, disposable.ServiceProvider);

            return await base.OnTestClassFinished(ctxt, summary);
        }
        finally
        {
            await disposable.DisposeAsync();
        }
    }

    /*protected override void CreateClassFixture(Type fixtureType)
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

        foreach (var fixture in ClassFixtureMappings.Values
                     .Where(x => x is not IDisposable and not IAsyncLifetime)
                     .OfType<IAsyncDisposable>())
            await Aggregator.RunAsync(() => fixture.DisposeAsync().AsTask());

        if (_serviceScope is { } disposable) await disposable.DisposeAsync();
    }*/

    // This method has been slightly modified from the original implementation to run tests in parallel
    // https://github.com/xunit/xunit/blob/v2-2.4.2/src/xunit.execution/Sdk/Frameworks/Runners/TestClassRunner.cs#L194-L219
    protected override async ValueTask<RunSummary> RunTestMethods(XunitTestClassRunnerContext ctxt,
        Exception? exception)
    {
        if (exception != null ||
            context.DisableParallelization ||
            ctxt.TestCases.Count < 2 ||
            ctxt.TestClass.Class.GetCustomAttribute<CollectionDefinitionAttribute>() is
            {
                DisableParallelization: true
            } ||
            ctxt.TestClass.Class.GetCustomAttribute<DisableParallelizationAttribute>() != null ||
            ctxt.TestClass.Class.GetCustomAttribute<CollectionAttribute>() != null && !context.ForcedParallelization)
            return await base.RunTestMethods(ctxt, exception);

        var summary = new RunSummary();
        IReadOnlyCollection<IXunitTestCase> orderedTestCases;
        object?[] constructorArguments;

        if (exception is null)
        {
            orderedTestCases = OrderTestCases(ctxt);
            constructorArguments = await CreateTestClassConstructorArguments(ctxt);
            exception = ctxt.Aggregator.ToException();
            ctxt.Aggregator.Clear();
        }
        else
        {
            orderedTestCases = ctxt.TestCases;
            constructorArguments = [];
        }

        if (exception != null) return await base.RunTestMethods(ctxt, exception);

        var methodTasks = orderedTestCases.GroupBy(tc => tc.TestMethod, TestMethodComparer.Instance)
            .Select(method => RunTestMethod(ctxt, method.Key as IXunitTestMethod, method.CastOrToReadOnlyCollection(),
                constructorArguments).AsTask());

        foreach (var methodSummary in await Task.WhenAll(methodTasks))
            summary.Aggregate(methodSummary);

        return summary;
    }

    /// <inheritdoc />
    protected override ValueTask<RunSummary> RunTestMethod(XunitTestClassRunnerContext ctxt,
        IXunitTestMethod? testMethod, IReadOnlyCollection<IXunitTestCase> testCases, object?[] constructorArguments) =>
        testMethod == null
            ? base.RunTestMethod(ctxt, testMethod, testCases, constructorArguments)
            : new DependencyInjectionTestMethodRunner(context).Run(
                testMethod,
                testCases,
                ctxt.ExplicitOption,
                ctxt.MessageBus,
                ctxt.Aggregator.Clone(),
                ctxt.CancellationTokenSource,
                constructorArguments);
}
