﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Reflection;

namespace Xunit.DependencyInjection
{
    internal static class StartupLoader
    {
        public static Type? GetStartupType(AssemblyName assemblyName)
        {
            var assembly = Assembly.Load(assemblyName);
            var attr = assembly.GetCustomAttribute<StartupTypeAttribute>();

            if (attr == null) return assembly.GetType($"{assemblyName.Name}.Startup");

            if (attr.AssemblyName != null) assembly = Assembly.Load(attr.AssemblyName);

            return assembly.GetType(attr.TypeName) ?? throw new InvalidOperationException($"Can't load type {attr.TypeName} in '{assembly.FullName}'");
        }

        public static object? CreateStartup(Type? startupType, AssemblyName assemblyName)
        {
            if (startupType == null) return null;

            var ctors = startupType.GetConstructors();
            if (ctors.Length > 1)
                throw new InvalidOperationException($"Having multiple constructors of startup type '{startupType.AssemblyQualifiedName}'");

            if (ctors.Length == 0) return Activator.CreateInstance(startupType);

            var parameters = ctors[0].GetParameters();
            if (parameters.Length == 0) return Activator.CreateInstance(startupType);

            if (parameters.Length > 1 || parameters[0].ParameterType != typeof(AssemblyName))
                throw new InvalidOperationException($"The constructor of startup type '{startupType.AssemblyQualifiedName}' must have no parameter or have the only 'AssemblyName' parameter.");

            return Activator.CreateInstance(startupType, assemblyName);
        }

        public static IHostBuilder ConfigureHost(IHostBuilder builder, object startup)
        {
            var method = FindMethod(startup.GetType(), nameof(ConfigureHost), false);
            if (method == null) return builder;

            var parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                if (typeof(IHostBuilder).IsAssignableFrom(method.ReturnType))
                {
                    return method.Invoke(startup, Array.Empty<object>()) as IHostBuilder ?? builder;
                }
                throw new InvalidOperationException($"The '{method.Name}' method of startup type '{startup.GetType().FullName}' must have the only 'IHostBuilder' parameter or return a 'IHostBuilder' instance.");
            }

            if (parameters.Length != 1 || parameters[0].ParameterType != typeof(IHostBuilder))
                throw new InvalidOperationException($"The '{method.Name}' method of startup type '{startup.GetType().FullName}' must have the only 'IHostBuilder' parameter.");

            if (method.ReturnType == typeof(void))
            {
                method.Invoke(startup, new object[] { builder });

                return builder;
            }

            if (typeof(IHostBuilder).IsAssignableFrom(method.ReturnType))
                return method.Invoke(startup, new object[] { builder }) as IHostBuilder ?? builder;

            throw new InvalidOperationException($"The '{method.Name}' method in the type '{startup.GetType().FullName}' must have no return type or return type must implement 'IHostBuilder'.");
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
                _ => throw new InvalidOperationException($"The '{method.Name}' method in the type '{startup.GetType().FullName}' must have a 'IServiceCollection' parameter and optional 'HostBuilderContext' parameter.")
            });
        }

        public static void Configure(IServiceProvider provider, object startup)
        {
            var method = FindMethod(startup.GetType(), nameof(Configure));

            method?.Invoke(startup, method.GetParameters().Select(p => provider.GetService(p.ParameterType)).ToArray());
        }

        private static MethodInfo? FindMethod(Type startupType, string methodName, bool validateReturnType = true)
        {
            var selectedMethods = startupType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(method => method.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (selectedMethods.Count > 1)
                throw new InvalidOperationException($"Having multiple overloads of method '{methodName}' is not supported.");

            var methodInfo = selectedMethods.FirstOrDefault();

            if (methodInfo != null && validateReturnType && methodInfo.ReturnType != typeof(void))
                throw new InvalidOperationException($"The '{methodInfo.Name}' method in the type '{startupType.FullName}' must have no return type.");

            return methodInfo;
        }
    }
}
