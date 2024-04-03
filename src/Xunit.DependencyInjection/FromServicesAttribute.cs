namespace Xunit.DependencyInjection;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromServicesAttribute() : FromKeyedServicesAttribute(null!)
{
    internal static IReadOnlyDictionary<int, ParameterInfo> CreateFromServices(MethodInfo method)
    {
        var dic = new Dictionary<int, ParameterInfo>();

        var parameters = method.GetParameters();
        for (var index = 0; index < parameters.Length; index++)
        {
            var parameter = parameters[index];
            if (parameter.ParameterType is { IsClass: false, IsInterface: false }) continue;

            if (parameter.GetCustomAttribute<FromKeyedServicesAttribute>() != null) dic[index] = parameter;
        }

        return dic;
    }
}
