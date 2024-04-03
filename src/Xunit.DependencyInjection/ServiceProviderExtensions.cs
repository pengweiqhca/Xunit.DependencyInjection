namespace Xunit.DependencyInjection;

internal static class ServiceProviderExtensions
{
    public static object GetRequiredService(this IServiceProvider provider, ParameterInfo parameter) =>
        parameter.GetCustomAttribute<FromKeyedServicesAttribute>() is { } attribute and not FromServicesAttribute
            ? provider.GetRequiredKeyedService(parameter.ParameterType, attribute.Key)
            : provider.GetRequiredService(parameter.ParameterType);

    public static object? GetService(this IServiceProvider provider, ParameterInfo parameter) =>
        parameter.GetCustomAttribute<FromKeyedServicesAttribute>() is { } attribute and not FromServicesAttribute
            ? ((IKeyedServiceProvider)provider).GetKeyedService(parameter.ParameterType, attribute.Key)
            : provider.GetService(parameter.ParameterType);
}
