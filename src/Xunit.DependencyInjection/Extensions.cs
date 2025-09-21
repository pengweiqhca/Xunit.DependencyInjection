using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Xunit.DependencyInjection;

internal static class Extensions
{
    extension(Type type)
    {
        public bool HasRequiredMemberAttribute()
        {
            for (var currentType = type; currentType is not null && currentType != typeof(object);
                 currentType = currentType.BaseType)
                if (currentType.CustomAttributes.Any(cad =>
                        cad.AttributeType.FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute"))
                    return true;

            return false;
        }
    }

    extension(PropertyInfo propertyInfo)
    {
        public bool HasRequiredMemberAttribute() => propertyInfo.CustomAttributes.Any(
            cad => cad.AttributeType.FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute");
    }

    extension(ConstructorInfo constructorInfo)
    {
        public bool HasSetsRequiredMembersAttribute() =>
            constructorInfo.CustomAttributes.Any(cad =>
                cad.AttributeType.FullName == "System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute");
    }

    extension(IServiceProvider provider)
    {
        public object GetRequiredService(ParameterInfo parameter) =>
            parameter.GetCustomAttribute<FromKeyedServicesAttribute>() is { } attribute and not FromServicesAttribute
                ? provider.GetRequiredKeyedService(parameter.ParameterType, attribute.Key)
                : provider.GetRequiredService(parameter.ParameterType);

        public object? GetService(ParameterInfo parameter) =>
            parameter.GetCustomAttribute<FromKeyedServicesAttribute>() is { } attribute and not FromServicesAttribute
                ? ((IKeyedServiceProvider)provider).GetKeyedService(parameter.ParameterType, attribute.Key)
                : provider.GetService(parameter.ParameterType);
    }
#if NETFRAMEWORK
    extension(ArgumentNullException)
    {
        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null) throw new ArgumentNullException(paramName);
        }
    }
#endif
}
