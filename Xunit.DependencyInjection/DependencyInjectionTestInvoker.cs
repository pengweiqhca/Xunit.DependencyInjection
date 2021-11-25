using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.DependencyInjection;

public class DependencyInjectionTestInvoker : XunitTestInvoker
{
    private static readonly ActivitySource ActivitySource = new("Xunit.DependencyInjection", typeof(DependencyInjectionTestInvoker).Assembly.GetName().Version.ToString());
    private static readonly MethodInfo AsTaskMethod = new Func<ValueTask<object>, Task>(AsTask).Method.GetGenericMethodDefinition();
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
                { "Type", TestCase.TestMethod.TestClass.Class.Name },
                { "Method", TestCase.Method.Name },
            });

        if (activity == null)
        {
            var result = base.CallTestMethod(testClassInstance);

            return TryAsTask(result, out var task) ? AsyncStack(task, activity) : result;
        }

        try
        {
            var result = base.CallTestMethod(testClassInstance);

            if (TryAsTask(result, out var task)) return AsyncStack(task, activity);

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
    }

    private static bool TryAsTask(object? result, [NotNullWhen(true)] out Task? task)
    {
        task = null;

        if (result == null) return false;

        if (result is Task t)
        {
            task = t;

            return true;
        }

        if (result is ValueTask vt)
        {
            task = vt.AsTask();

            return true;
        }

        var type = result.GetType();
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(ValueTask<>)) return false;

        task = (Task)AsTaskMethod.MakeGenericMethod(type.GetGenericArguments()[0]).Invoke(null, new[] { result });

        return true;
    }

    private static Task AsTask<T>(ValueTask<T> task) => task.AsTask();

    private async Task AsyncStack(Task task, Activity? activity)
    {
        try
        {
            await task.ConfigureAwait(false);

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
