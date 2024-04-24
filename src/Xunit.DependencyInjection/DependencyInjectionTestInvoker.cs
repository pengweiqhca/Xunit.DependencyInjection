using Microsoft.Extensions.Internal;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Xunit.DependencyInjection;

public class DependencyInjectionTestInvoker(
    IServiceProvider provider,
    ITest test,
    IMessageBus messageBus,
    Type testClass,
    object?[] constructorArguments,
    MethodInfo testMethod,
    object[] testMethodArguments,
    IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource)
    : XunitTestInvoker(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments,
        beforeAfterAttributes, aggregator, cancellationTokenSource)
{
    private static readonly ActivitySource ActivitySource = new("Xunit.DependencyInjection", typeof(DependencyInjectionTestInvoker).Assembly.GetName().Version?.ToString());
    private static readonly MethodInfo AsTaskMethod = new Func<ObjectMethodExecutorAwaitable, Task>(AsTask).Method;

    /// <inheritdoc/>
    protected override async Task<decimal> InvokeTestMethodAsync(object? testClassInstance)
    {
        var beforeAfterTests = provider.GetServices<BeforeAfterTest>().ToArray();

        foreach (var beforeAfterTest in beforeAfterTests)
            await beforeAfterTest.BeforeAsync(testClassInstance, TestMethod).ConfigureAwait(false);

        var result = await base.InvokeTestMethodAsync(testClassInstance).ConfigureAwait(false);

        for (var index = beforeAfterTests.Length - 1; index >= 0; index--)
            await beforeAfterTests[index].AfterAsync(testClassInstance, TestMethod).ConfigureAwait(false);

        return result;
    }

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

            return TryAsTask(TestCase.Method.ReturnType, result, out var task) ? AsyncStack(task, activity) : result;
        }

        try
        {
            var result = base.CallTestMethod(testClassInstance);

            if (TryAsTask(TestCase.Method.ReturnType, result, out var task)) return AsyncStack(task, activity);

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

    private static readonly ConcurrentDictionary<ITypeInfo, Func<object, Task>?> Factories = new();

    private static bool TryAsTask(ITypeInfo typeInfo, object? result, [NotNullWhen(true)] out Task? task)
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

        var func = Factories.GetOrAdd(typeInfo, static typeInfo =>
        {
            var type = typeInfo.ToRuntimeType();

            if (!CoercedAwaitableInfo.IsTypeAwaitable(type, out var coercedAwaitableInfo)) return null;

            var param = Expression.Parameter(typeof(object));

            return Expression.Lambda<Func<object, Task>>(Expression.Call(AsTaskMethod,
                ObjectMethodExecutor.ConvertToObjectMethodExecutorAwaitable(coercedAwaitableInfo,
                    Expression.Convert(param, type))), param).Compile();
        });

        if (func == null) return false;

        task = func(result);

        return true;
    }

    private static async Task AsTask(ObjectMethodExecutorAwaitable awaitable) => await awaitable;

    private async Task AsyncStack(Task task, Activity? activity)
    {
        try
        {
            await task;

            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            ex = ex.Unwrap();

            Aggregator.Add(provider.GetService<IAsyncExceptionFilter>()?.Process(ex) ?? ex);

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        }

        activity?.Stop();
    }
}
