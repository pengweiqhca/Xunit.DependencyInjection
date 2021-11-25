using System.Diagnostics;

namespace Xunit.DependencyInjection;

public class DependencyInjectionTestInvoker : XunitTestInvoker
{
    private static readonly ActivitySource ActivitySource = new("Xunit.DependencyInjection", typeof(DependencyInjectionTestInvoker).Assembly.GetName().Version.ToString());
    private readonly IServiceProvider _provider;

    public DependencyInjectionTestInvoker(IServiceProvider provider, ITest test, IMessageBus messageBus,
        Type testClass, object?[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments,
        IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
        : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments,
            beforeAfterAttributes, aggregator, cancellationTokenSource) =>
        _provider = provider;

    /// <inheritdoc />
    protected override object CallTestMethod(object testClassInstance)
    {
        var activity = ActivitySource.StartActivity(TestCase.DisplayName, ActivityKind.Internal,
            Activity.Current?.Context ?? default, new Dictionary<string, object?>
            {
                { "Type", TestCase.Method.Type.Name },
                { "Method", TestCase.Method.Name },
            });

        if (activity == null)
        {
            var result = base.CallTestMethod(testClassInstance);

            return result is Task task ? AsyncStack(task) : result;
        }

        try
        {
            var result = base.CallTestMethod(testClassInstance);

            if (result is Task task) return AsyncStack(task);

            activity.SetStatus(ActivityStatusCode.Ok);

            activity.Stop();

            return result;
        }
        catch (Exception ex)
        {
            activity.SetStatus(ActivityStatusCode.Error, ex.Message);

            activity.Stop();

            throw;
        }

        async Task AsyncStack(Task t)
        {
            try
            {
                await t.ConfigureAwait(false);

                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                while (ex is TargetInvocationException { InnerException: { } } tie)
                {
                    ex = tie.InnerException;
                }

                Aggregator.Add(_provider.GetService<IAsyncExceptionFilter>()?.Process(ex) ?? ex);

                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            }

            activity?.Stop();
        }
    }
}
