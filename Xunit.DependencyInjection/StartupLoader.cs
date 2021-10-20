using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Reflection;

namespace Xunit.DependencyInjection
{
    internal static class StartupLoader
    {
        public static (Type StartupType, Type ModuleType)[] GetModuleStartupTypes(Type mainAssemblyStartupType)
        {
            bool IsValidModuleStartup(Type startupType)
            {
                if (startupType == mainAssemblyStartupType) return false;

                if (!(startupType.IsNested && startupType.DeclaringType is { IsAbstract: true, IsSealed: true }))
                    return false;

                try
                {
                    return
                        FindMethod(startupType, nameof(CreateHostBuilder), typeof(IHostBuilder)) is not null
                     || FindMethod(startupType, nameof(ConfigureHost)) is not null
                     || FindMethod(startupType, nameof(ConfigureServices)) is not null
                     || FindMethod(startupType, nameof(Configure)) is not null;
                }
                catch
                {
                    return false;
                }
            }

            var types = mainAssemblyStartupType.Assembly.GetTypes();

            return
                types
                   .Where(x => x.Name.Equals("Startup") && IsValidModuleStartup(x))
                   .Select(x => (x, x.DeclaringType))
                   .ToArray();
        }

        public static Type GetAssemblyStartupType(AssemblyName assemblyName)
        {
            var assembly = Assembly.Load(assemblyName);
            var attr = assembly.GetCustomAttribute<StartupTypeAttribute>();

            if (attr == null) return assembly.GetType($"{assemblyName.Name}.Startup");

            if (attr.AssemblyName != null) assembly = Assembly.Load(attr.AssemblyName);

            return assembly.GetType(attr.TypeName) ?? throw new InvalidOperationException($"Can't load type {attr.TypeName} in '{assembly.FullName}'");
        }

        public static object? CreateStartup(Type? startupType)
        {
            if (startupType == null) return null;

            var ctors = startupType.GetConstructors();
            if (ctors.Length != 1 || ctors[0].GetParameters().Length != 0)
                throw new InvalidOperationException($"'{startupType.FullName}' must have a single parameterless public constructor.");

            return Activator.CreateInstance(startupType);
        }

        public static IHostBuilder? CreateHostBuilder(object startup, AssemblyName assemblyName)
        {
            var method = FindMethod(startup.GetType(), nameof(CreateHostBuilder), typeof(IHostBuilder));
            if (method == null) return null;

            var parameters = method.GetParameters();
            if (parameters.Length == 0)
                return (IHostBuilder)method.Invoke(startup, Array.Empty<object>());

            if (parameters.Length > 1 || parameters[0].ParameterType != typeof(AssemblyName))
                throw new InvalidOperationException($"The '{method.Name}' method of startup type '{startup.GetType().FullName}' must parameterless or have the single 'AssemblyName' parameter.");

            return (IHostBuilder)method.Invoke(startup, new object[] { assemblyName });
        }

        public static void ConfigureHost(IHostBuilder builder, object startup)
        {
            var method = FindMethod(startup.GetType(), nameof(ConfigureHost));
            if (method == null) return;

            var parameters = method.GetParameters();
            if (parameters.Length != 1 || parameters[0].ParameterType != typeof(IHostBuilder))
                throw new InvalidOperationException($"The '{method.Name}' method of startup type '{startup.GetType().FullName}' must have the single 'IHostBuilder' parameter.");

            method.Invoke(startup, new object[] { builder });
        }

        public static void ConfigureServices(IHostBuilder builder, object startup)
        {
            var method = FindMethod(startup.GetType(), nameof(ConfigureServices));
            if (method == null) return;

            var parameters = method.GetParameters();
            builder.ConfigureServices(parameters.Length switch
            {
                1 when parameters[0].ParameterType == typeof(IServiceCollection) =>
                    (context, services) => method.Invoke(startup, new object[] { services }),
                2 when parameters[0].ParameterType == typeof(IServiceCollection) &&
                       parameters[1].ParameterType == typeof(HostBuilderContext) =>
                    (context, services) => method.Invoke(startup, new object[] { services, context }),
                2 when parameters[1].ParameterType == typeof(IServiceCollection) &&
                       parameters[0].ParameterType == typeof(HostBuilderContext) =>
                    (context, services) => method.Invoke(startup, new object[] { context, services }),
                _ => throw new InvalidOperationException(
                    $"The '{method.Name}' method in the type '{startup.GetType().FullName}' must have a 'IServiceCollection' parameter and optional 'HostBuilderContext' parameter.")
            });
        }

        public static void Configure(IServiceProvider provider, object startup)
        {
            var method = FindMethod(startup.GetType(), nameof(Configure));

            method?.Invoke(startup, method.GetParameters().Select(p => provider.GetService(p.ParameterType)).ToArray());
        }

        private static MethodInfo? FindMethod(Type startupType, string methodName) =>
            FindMethod(startupType, methodName, typeof(void));

        private static MethodInfo? FindMethod(Type startupType, string methodName, Type returnType)
        {
            var selectedMethods = startupType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                                             .Where(method => method.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (selectedMethods.Count > 1)
                throw new InvalidOperationException($"Having multiple overloads of method '{methodName}' is not supported.");

            var methodInfo = selectedMethods.FirstOrDefault();
            if (methodInfo == null) return methodInfo;

            if (methodInfo.IsStatic)
                throw new InvalidOperationException($"Method '{methodName}' should not be static.");

            if (returnType == typeof(void))
            {
                if (methodInfo.ReturnType != returnType)
                    throw new InvalidOperationException($"The '{methodInfo.Name}' method in the type '{startupType.FullName}' must have no return type.");
            }
            else if (!returnType.IsAssignableFrom(methodInfo.ReturnType))
                throw new InvalidOperationException($"The '{methodInfo.Name}' method in the type '{startupType.FullName}' return type must assignable to '{returnType}'.");

            return methodInfo;
        }
    }
}
