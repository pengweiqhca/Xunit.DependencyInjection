using Microsoft.Extensions.Internal;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;

namespace Xunit.DependencyInjection;

internal static class TestHelper
{
    private static readonly MethodInfo AsTaskMethod;
    private static readonly MethodInfo AsTaskResultMethod;

    static TestHelper()
    {
        var asTask = AsTask;
        var asTaskResult = AsTaskResult;

        AsTaskMethod = asTask.Method;
        AsTaskResultMethod = asTaskResult.Method;
    }

    public static object?[] CreateTestClassConstructorArguments(this IServiceProvider provider,
        object?[] constructorArguments, ExceptionAggregator aggregator)
    {
        var unusedArguments = new List<Tuple<int, ParameterInfo>>();
        Func<IReadOnlyList<Tuple<int, ParameterInfo>>, string>? formatConstructorArgsMissingMessage = null;

        var args = new object?[constructorArguments.Length];
        for (var index = 0; index < constructorArguments.Length; index++)
            if (constructorArguments[index] is DelayArgument delay)
            {
                formatConstructorArgsMissingMessage = delay.FormatConstructorArgsMissingMessage;

                if (delay.TryGetConstructorArgument(provider, aggregator, out var arg))
                    args[index] = arg;
                else
                    unusedArguments.Add(Tuple.Create(index, delay.Parameter));
            }
            else
                args[index] = constructorArguments[index] is TestOutputHelperArgument
                    ? provider.GetRequiredService<ITestOutputHelperAccessor>().Output
                    : constructorArguments[index];

        if (unusedArguments.Count > 0 && formatConstructorArgsMissingMessage != null)
            aggregator.Add(new TestPipelineException(formatConstructorArgsMissingMessage(unusedArguments)));

        return args;
    }

    public class DelayArgument(
        ParameterInfo parameter,
        Func<IReadOnlyList<Tuple<int, ParameterInfo>>, string> formatConstructorArgsMissingMessage)
    {
        public ParameterInfo Parameter { get; } = parameter;

        public Func<IReadOnlyList<Tuple<int, ParameterInfo>>, string> FormatConstructorArgsMissingMessage { get; } =
            formatConstructorArgsMissingMessage;

        public bool TryGetConstructorArgument(IServiceProvider provider, ExceptionAggregator aggregator,
            out object? argumentValue)
        {
            argumentValue = null;

            try
            {
                argumentValue = provider.GetService(Parameter);
            }
            catch (Exception ex)
            {
                aggregator.Add(ex);

                return true;
            }

            if (argumentValue != null)
                return true;

            if (Parameter.HasDefaultValue)
                argumentValue = Parameter.DefaultValue;
            else if (Parameter.IsOptional)
                argumentValue = Parameter.ParameterType.GetDefaultValue();
            else if (Parameter.GetCustomAttribute<ParamArrayAttribute>() != null)
                argumentValue = Array.CreateInstance(Parameter.ParameterType, 0);
            else
                return false;

            return true;
        }
    }

    public class TestOutputHelperArgument
    {
        private TestOutputHelperArgument() { }

        public static TestOutputHelperArgument Instance { get; } = new();
    }

    public static Exception Unwrap(this Exception ex)
    {
        while (ex is TargetInvocationException { InnerException: not null } tie) ex = tie.InnerException!;

        while (ex is AggregateException { InnerException: not null } ae) ex = ae.InnerException!;

        return ex;
    }

    public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> source, Func<T, bool> predicate) =>
        source.Where(item => !predicate(item));

    public static bool GenericTypeArgumentIsGenericParameter(Type type) =>
        type.IsGenericType && type.GenericTypeArguments.Any(t => t.IsGenericParameter);

    private static IDictionary<Type, object> GetFixtureCache(this FixtureMappingManager manager)
    {
        var field =
            typeof(FixtureMappingManager).GetField("fixtureCache", BindingFlags.Instance | BindingFlags.NonPublic) ??
            throw new NotSupportedException("Not found `fixtureCache` field in FixtureMappingManager");

        return field.GetValue(manager) as Dictionary<Type, object> ??
            throw new NotSupportedException("`fixtureCache` is not a Dictionary<Type, object>");
    }

    public static async ValueTask CreateFixtures(this FixtureMappingManager manager, IReadOnlyCollection<Type> fixtureTypes, ExceptionAggregator aggregator, IServiceProvider provider)
    {
        var field = typeof(FixtureMappingManager).GetField("parentMappingManager", BindingFlags.Instance | BindingFlags.NonPublic) ??
            throw new NotSupportedException("Not found `parentMappingManager` field in FixtureMappingManager");

        var value = field.GetValue(manager);
        if (value != null && value is not FixtureMappingManager)
            throw new NotSupportedException("`parentMappingManager` is not a FixtureMappingManager");

        var parentMappingManager = value as FixtureMappingManager;

        field = typeof(FixtureMappingManager).GetField("knownTypes", BindingFlags.Instance | BindingFlags.NonPublic) ??
            throw new NotSupportedException("Not found `knownTypes` field in FixtureMappingManager");

        if (field.GetValue(manager) is not HashSet<Type> knownTypes)
            throw new NotSupportedException("`knownTypes` is not a HashSet<Type>");

        field = typeof(FixtureMappingManager).GetField("fixtureCategory", BindingFlags.Instance | BindingFlags.NonPublic) ??
            throw new NotSupportedException("Not found `fixtureCategory` field in FixtureMappingManager");

        if (field.GetValue(manager) is not string fixtureCategory)
            throw new NotSupportedException("`fixtureCategory` is not a string");

        var fixtureCache = manager.GetFixtureCache();

        foreach (var fixtureType in fixtureTypes)
            await aggregator.RunAsync(async () =>
            {
                knownTypes.Add(fixtureType);

                fixtureCache[fixtureType] = provider.GetService(fixtureType) ??
                    await GetFixture(fixtureType, parentMappingManager, provider, fixtureCategory);
            });
    }

    public static void ClearFixtures(this FixtureMappingManager manager, IReadOnlyCollection<Type> fixtureTypes,
        IServiceProvider provider)
    {
        var serviceProviderIsService = provider.GetRequiredService<IServiceProviderIsService>();
        var fixtureCache = manager.GetFixtureCache();

        foreach (var type in fixtureTypes)
            if (serviceProviderIsService.IsService(type))
                fixtureCache.Remove(type);
    }

    private static async ValueTask<object> GetFixture(Type fixtureType, FixtureMappingManager? parentMappingManager,
        IServiceProvider provider, string fixtureCategory)
    {
        // Ensure there is a single public constructor
        var ctors = fixtureType
            .GetConstructors()
            .Where(ci => !ci.IsStatic && ci.IsPublic)
            .ToList();

        if (ctors.Count != 1)
            throw new TestPipelineException(string.Format(CultureInfo.CurrentCulture,
                "{0} fixture type '{1}' may only define a single public constructor.", fixtureCategory,
                fixtureType.SafeName()));

        // Make sure we can accommodate all the constructor arguments from either known types or the parent
        var ctor = ctors[0];
        var parameters = ctor.GetParameters();
        var ctorArgs = new object[parameters.Length];
        var ctorIdx = 0;
        var missingParameters = new List<ParameterInfo>();

        foreach (var parameter in parameters)
        {
            /*if (parameter.ParameterType == typeof(IMessageSink))
                arg = TestContext.CurrentInternal.DiagnosticMessageSink ?? NullMessageSink.Instance;*/
            object? arg = null;
            if (provider.GetService(parameter.ParameterType) is { } service)
                arg = service;
            else if (parentMappingManager is not null)
                arg = await parentMappingManager.GetFixture(parameter.ParameterType);

            if (arg is null) missingParameters.Add(parameter);
            else ctorArgs[ctorIdx++] = arg;
        }

        if (missingParameters.Count > 0)
            throw new TestPipelineException(string.Format(
                CultureInfo.CurrentCulture,
                "{0} fixture type '{1}' had one or more unresolved constructor arguments: {2}",
                fixtureCategory,
                fixtureType.SafeName(),
                string.Join(", ", missingParameters.Select(p =>
                    string.Format(CultureInfo.CurrentCulture, "{0} {1}", p.ParameterType.Name, p.Name)))));

        object result;
        try
        {
            result = ctor.Invoke(ctorArgs);
        }
        catch (Exception ex)
        {
            throw new TestPipelineException(
                string.Format(CultureInfo.CurrentCulture, "{0} fixture type '{1}' threw in its constructor",
                    fixtureCategory, fixtureType.SafeName()), ex.Unwrap());
        }

        // Do async initialization
        if (result is IAsyncLifetime asyncLifetime)
            try
            {
                await asyncLifetime.InitializeAsync();
            }
            catch (Exception ex)
            {
                throw new TestPipelineException(
                    string.Format(CultureInfo.CurrentCulture, "{0} fixture type '{1}' threw in InitializeAsync",
                        fixtureCategory, fixtureType.SafeName()), ex.Unwrap());
            }

        return result;
    }

    private static readonly ConcurrentDictionary<Type, Func<object, Task>?> Factories = new();

    public static async Task<object?> TryAwait(Type type, object? result)
    {
        if (result == null) return null;

        var func = Factories.GetOrAdd(type, static type =>
        {
            if (!CoercedAwaitableInfo.IsTypeAwaitable(type, out var coercedAwaitableInfo)) return null;

            var param = Expression.Parameter(typeof(object));

            return Expression.Lambda<Func<object, Task>>(Expression.Call(
                coercedAwaitableInfo.AwaitableInfo.AwaiterGetResultMethod.ReturnType == typeof(void)
                    ? AsTaskMethod
                    : AsTaskResultMethod,
                ObjectMethodExecutor.ConvertToObjectMethodExecutorAwaitable(coercedAwaitableInfo,
                    Expression.Convert(param, type))), param).Compile();
        });

        if (func == null) return result;

        var task = func(result);

        if (task is Task<object?> taskResult) return await taskResult;

        await task;

        return null;
    }

    private static async Task AsTask(ObjectMethodExecutorAwaitable awaitable) => await awaitable;

    private static async Task<object?> AsTaskResult(ObjectMethodExecutorAwaitable awaitable) => await awaitable;
}
