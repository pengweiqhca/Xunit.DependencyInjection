namespace Xunit.DependencyInjection;

public class DependencyInjectionTestClassRunner(DependencyInjectionContext context)
    : XunitTestClassRunner
{
    private AsyncServiceScope? _serviceScope;

    /// <inheritdoc />
    protected override ValueTask<object?> GetConstructorArgument(XunitTestClassRunnerContext ctxt,
        ConstructorInfo constructor, int index, ParameterInfo parameter)
    {
        if (parameter.ParameterType == typeof(ITestOutputHelper))
            return new(TestHelper.TestOutputHelperArgument.Instance);

        return parameter.ParameterType == typeof(CancellationToken)
            ? new(ctxt.CancellationTokenSource.Token)
            : base.GetConstructorArgument(ctxt, constructor, index, parameter);
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

    /// <inheritdoc />
    protected override ValueTask<RunSummary> RunTestMethod(XunitTestClassRunnerContext ctxt,
        IXunitTestMethod? testMethod, IReadOnlyCollection<IXunitTestCase> testCases) =>
        testMethod == null
            ? base.RunTestMethod(ctxt, testMethod, testCases)
            : new DependencyInjectionTestMethodRunner(context).Run(
                testMethod,
                testCases,
                ctxt.ExplicitOption,
                ctxt.MessageBus,
                ctxt.Aggregator.Clone(),
                ctxt.CancellationTokenSource,
                context.DisableParallelization ||
                testMethod.Method.GetCustomAttribute<DisableParallelizationAttribute>() is not null
                    ? ParallelMode.None
                    : ctxt.ParallelMode,
                ctxt.Scheduler,
                ctxt.ConstructorArguments ?? [],
                ctxt.ClassFixtureMappings);
}
