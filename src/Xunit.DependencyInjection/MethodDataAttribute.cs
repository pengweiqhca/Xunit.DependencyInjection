using System.ComponentModel;

namespace Xunit.DependencyInjection;

/// <summary>
/// Provides a data source for a data theory.
/// The member must return something compatible with IEnumerable&lt;object[]&gt; with the test data.
/// </summary>
/// <param name="methodName">The name of the public method on the test class that will provide the test data</param>
/// <param name="parameters">The parameters for the method</param>
[DataDiscoverer("Xunit.Sdk.MemberDataDiscoverer", "xunit.core")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class MethodDataAttribute(string methodName, params object?[] parameters) : DataAttribute
{
    /// <summary>
    /// Gets the method name.
    /// </summary>
    public string MethodName { get; } = methodName;

    /// <summary>
    /// Gets or sets the parameters passed to the member. Only supported for static methods.
    /// </summary>
    public object?[]? Parameters { get; } = parameters;

    /// <summary>
    /// Gets the type of the class that provides the data.
    /// </summary>
    public Type? ClassType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodDataAttribute" /> class.
    /// </summary>
    /// <param name="methodName">The name of the public method on the test class that will provide the test data</param>
    /// <param name="classType">The class that provides the data.</param>
    /// <param name="parameters">The parameters for the method</param>
    public MethodDataAttribute(string methodName, Type classType, params object?[] parameters)
        : this(methodName, parameters) => ClassType = classType;

    /// <inheritdoc />
    public override IEnumerable<object?[]?>? GetData(MethodInfo testMethod)
    {
        var provider = TheoryTestCaseDataContext.Services ?? throw new InvalidOperationException("Please use MethodDataAttribute in injected class by Xunit.DependencyInjection");

        var type = ClassType ?? testMethod.DeclaringType ?? testMethod.ReflectedType!;
        var method = GetMethodInfo(type);
        if (method == null)
        {
            var parameterText = Parameters is { Length: > 0 } ? $" with parameter types: {string.Join(", ", Parameters.Select(p => p?.GetType().FullName ?? "(null)"))}" : "";

            throw new ArgumentException($"Could not find public method named '{MethodName}' on {type.FullName}{parameterText}");
        }

        var result = method.Invoke(method.IsStatic ? null : ActivatorUtilities.CreateInstance(provider, type), GetParameters(provider, method));

        return result switch
        {
            null => null,
            IEnumerable<object?> dataItems => dataItems.Select(item => item switch
            {
                null => null,
                object?[] array => array,
                _ => throw new ArgumentException($"Method {MethodName} on {type.FullName} yielded an item that is not an object[]")
            }),
            _ => throw new ArgumentException($"Method {MethodName} on {type.FullName} did not return IEnumerable<object>")
        };
    }

    private MethodInfo? GetMethodInfo(Type type)
    {
        var parameterTypes = Parameters == null ? Type.EmptyTypes : Parameters.Select(p => p?.GetType()).ToArray();

        for (var reflectionType = type; reflectionType != null; reflectionType = reflectionType.GetTypeInfo().BaseType)
        {
            var methodInfo = reflectionType.GetRuntimeMethods()
                .FirstOrDefault(m => m.Name == MethodName &&
                                     ParameterTypesCompatible(m.GetParameters(), parameterTypes));

            if (methodInfo != null) return methodInfo;
        }

        return null;
    }

    private static bool ParameterTypesCompatible(IReadOnlyCollection<ParameterInfo> parameters, IReadOnlyList<Type?> parameterTypes)
    {
        if (parameterTypes.Count < 1) return true;

        if (parameters.Count != parameterTypes.Count) return false;

        return !parameters.Where((t, idx) => parameterTypes[idx] != null && !t.ParameterType.IsAssignableFrom(parameterTypes[idx])).Any();
    }

    private object?[] GetParameters(IServiceProvider serviceProvider, MethodBase method)
    {
        var mp = method.GetParameters();

        var param = Parameters == null || Parameters.Length == 0 ? new object?[mp.Length] : [..Parameters];

        for (var index = 0; index < param.Length; index++)
        {
            if (param[index] != null) continue;

            if (Parameters == null || Parameters.Length == 0 ||
                mp[index].GetCustomAttribute<FromKeyedServicesAttribute>() != null)
                param[index] = serviceProvider.GetService(mp[index]);
        }

        return param;
    }
}
