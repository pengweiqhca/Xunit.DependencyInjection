using System;
using System.Collections.Generic;
using System.Reflection;

namespace Xunit.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromServicesAttribute : Attribute
    {
        internal static IReadOnlyDictionary<int, Type> CreateFromServices(MethodInfo method)
        {
            var dic = new Dictionary<int, Type>();

            var parameters = method.GetParameters();
            for (var index = 0; index < parameters.Length; index++)
            {
                var type = parameters[index].ParameterType;
                if ((type.IsClass || type.IsInterface) &&
                    parameters[index].GetCustomAttribute<FromServicesAttribute>() != null)
                {
                    dic[index] = type;
                }
            }

            return dic;
        }
    }
}
