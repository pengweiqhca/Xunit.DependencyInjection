using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;

namespace Xunit.DependencyInjection;

public class DependencyInjectionTestRunner(
    DependencyInjectionContext context,
    IReadOnlyDictionary<int, ParameterInfo> fromServices)
    : XunitTestRunner
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> HasRequiredMembers = [];

    protected override async
        ValueTask<(object? Instance, SynchronizationContext? SyncContext, ExecutionContext? ExecutionContext)>
        CreateTestClassInstance(XunitTestRunnerContext ctxt)
    {
        var (testClassInstance, syncContext, executionContext) = await base.CreateTestClassInstance(ctxt);

        if (testClassInstance == null || ctxt.TestMethod.ReflectedType == null ||
            HasRequiredMembers.GetOrAdd(ctxt.TestMethod.ReflectedType, GetRequiredProperties) is not
            {
                Length: > 0
            } properties)
            return (testClassInstance, syncContext, executionContext);

        var provider = ((DependencyInjectionTestRunnerContext)ctxt).Provider;

        foreach (var propertyInfo in properties)
            propertyInfo.SetValue(testClassInstance, propertyInfo.PropertyType == typeof(ITestOutputHelper)
                ? TestContext.Current.TestOutputHelper
                : propertyInfo.PropertyType == typeof(CancellationToken)
                    ? ctxt.CancellationTokenSource.Token
                    : provider.GetRequiredService(propertyInfo.PropertyType));

        return (testClassInstance, syncContext, executionContext);
    }

    private static PropertyInfo[] GetRequiredProperties(Type testClass) =>
        !testClass.HasRequiredMemberAttribute() || testClass.GetConstructors().FirstOrDefault(static ci =>
            ci is { IsStatic: false, IsPublic: true }) is not { } ci || ci.HasSetsRequiredMembersAttribute()
            ? []
            : testClass.GetProperties()
                .Where(p => p.SetMethod is { IsPublic: true } && p.HasRequiredMemberAttribute())
                .ToArray();

    protected override async ValueTask<TimeSpan> RunTest(XunitTestRunnerContext ctxt)
    {
        await using var scope = context.RootServices.CreateAsyncScope();

        context.RootServices.GetRequiredService<DependencyInjectionTypeActivator>().Services = scope.ServiceProvider;

        var raw = new Dictionary<int, object?>(ctxt.TestMethodArguments.Length);
        foreach (var kv in fromServices)
        {
            raw[kv.Key] = ctxt.TestMethodArguments[kv.Key];

            ctxt.TestMethodArguments[kv.Key] = kv.Value.ParameterType == typeof(ITestOutputHelper)
                ? TestContext.Current.TestOutputHelper
                : scope.ServiceProvider.GetService(kv.Value);
        }

        try
        {
            return await base.RunTest(new DependencyInjectionTestRunnerContext(
                scope.ServiceProvider,
                ctxt.Test,
                ctxt.MessageBus,
                ctxt.ExplicitOption,
                ctxt.Aggregator,
                ctxt.CancellationTokenSource,
                ctxt.BeforeAfterTestAttributes,
                ctxt.ConstructorArguments
            ));
        }
        finally
        {
            foreach (var kv in raw)
                ctxt.TestMethodArguments[kv.Key] = kv.Value;
        }
    }

    /// <inheritdoc />
    protected override async ValueTask<TimeSpan> InvokeTest(XunitTestRunnerContext ctxt, object? testClassInstance)
    {
        var beforeAfterTests = ((DependencyInjectionTestRunnerContext)ctxt).Provider.GetServices<BeforeAfterTest>()
            .ToArray();

        foreach (var beforeAfterTest in beforeAfterTests)
            await beforeAfterTest.BeforeAsync(testClassInstance, ctxt.TestMethod);

        var result = await base.InvokeTest(ctxt, testClassInstance);

        for (var index = beforeAfterTests.Length - 1; index >= 0; index--)
            await beforeAfterTests[index].AfterAsync(testClassInstance, ctxt.TestMethod);

        return result;
    }

    private sealed class DependencyInjectionTestRunnerContext(
        IServiceProvider provider,
        IXunitTest test,
        IMessageBus messageBus,
        ExplicitOption explicitOption,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterTestAttributes,
        object?[] constructorArguments)
        : XunitTestRunnerContext(ConvertMethod(test, provider.GetService<IAsyncExceptionFilter>()), messageBus,
            explicitOption, aggregator, cancellationTokenSource, beforeAfterTestAttributes, constructorArguments)
    {
        public IServiceProvider Provider { get; } = provider;

        private static IXunitTest ConvertMethod(IXunitTest test, IAsyncExceptionFilter? exceptionFilter)
        {
            if (test.TestMethod is not XunitTestMethod testMethod) return test;

            var field = typeof(XunitTestMethod).GetField("method", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || field.GetValue(testMethod) is not MethodInfo methodInfo ||
                methodInfo is DependencyInjectionMethodInfo) return test;

            field.SetValue(testMethod, new DependencyInjectionMethodInfo(test.TestCase, exceptionFilter, methodInfo));

            return test;
        }
    }

    private sealed class DependencyInjectionMethodInfo(
        ITestCase testCase,
        IAsyncExceptionFilter? exceptionFilter,
        MethodInfo methodInfo) : MethodInfo
    {
        private static readonly ActivitySource ActivitySource = new("Xunit.DependencyInjection",
            typeof(TestHelper).Assembly.GetName().Version?.ToString());

        public override object[] GetCustomAttributes(bool inherit) => methodInfo.GetCustomAttributes(inherit);

        public override bool IsDefined(Type attributeType, bool inherit) =>
            methodInfo.IsDefined(attributeType, inherit);

        public override ParameterInfo[] GetParameters() => methodInfo.GetParameters();

        public override MethodImplAttributes GetMethodImplementationFlags() =>
            methodInfo.GetMethodImplementationFlags();

        public override object Invoke(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? parameters,
            CultureInfo? culture)
        {
            var activity = ActivitySource.StartActivity(testCase.TestCaseDisplayName,
                ActivityKind.Internal, Activity.Current?.Context ?? default, new Dictionary<string, object?>
                {
                    { "Type", testCase.TestClassName },
                    { "Method", testCase.TestMethodName },
                });

            var result = methodInfo.Invoke(obj, invokeAttr, binder, parameters, culture);

            var task = TestHelper.TryAwait(methodInfo.ReturnType, result);

            return activity == null ? task : AsyncStack(task, activity);
        }

        private async Task<object?> AsyncStack(Task<object?> task, Activity activity)
        {
            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                ex = ex.Unwrap();

                activity.SetStatus(ActivityStatusCode.Error, ex.Message)
                    .AddException(exceptionFilter?.Process(ex) ?? ex);

                throw;
            }
            finally
            {
                activity.Stop();
            }
        }

        public override MethodInfo GetBaseDefinition() => methodInfo.GetBaseDefinition();

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => methodInfo.ReturnTypeCustomAttributes;

        public override string Name => methodInfo.Name;

        public override Type? DeclaringType => methodInfo.DeclaringType;

        public override Type? ReflectedType => methodInfo.ReflectedType;

        public override RuntimeMethodHandle MethodHandle => methodInfo.MethodHandle;

        public override MethodAttributes Attributes => methodInfo.Attributes;

        public override Type ReturnType => typeof(Task<object?>);

        public override int MetadataToken => methodInfo.MetadataToken;

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) =>
            methodInfo.GetCustomAttributes(attributeType, inherit);
    }
}
