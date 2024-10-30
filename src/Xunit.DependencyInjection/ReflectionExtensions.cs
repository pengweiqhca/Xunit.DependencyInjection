namespace Xunit.DependencyInjection;

internal static class ReflectionExtensions
{
    public static bool HasRequiredMemberAttribute(this Type type)
    {
        for (var currentType = type; currentType is not null && currentType != typeof(object);
             currentType = currentType.BaseType)
            if (currentType.CustomAttributes.Any(cad =>
                    cad.AttributeType.FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute"))
                return true;

        return false;
    }

    public static bool HasRequiredMemberAttribute(this PropertyInfo propertyInfo) => propertyInfo.CustomAttributes.Any(
        cad => cad.AttributeType.FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute");

    public static bool HasSetsRequiredMembersAttribute(this ConstructorInfo constructorInfo) =>
        constructorInfo.CustomAttributes.Any(cad =>
            cad.AttributeType.FullName == "System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute");
}
