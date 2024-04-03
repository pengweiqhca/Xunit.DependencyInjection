using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Xunit.DependencyInjection;

internal static class StartupLoader
{
    public static DependencyInjectionContext CreateHost(Type startupType, AssemblyName? assemblyName,
        IMessageSink? diagnosticMessageSink)
    {
        var configureHostApplicationBuilderMethodInfo = FindMethod(startupType, "ConfigureHostApplicationBuilder");
        if (configureHostApplicationBuilderMethodInfo != null)
        {
            var parameters = configureHostApplicationBuilderMethodInfo.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IHostApplicationBuilder))
            {
                return CreateHostWithHostApplicationBuilder(startupType, configureHostApplicationBuilderMethodInfo, assemblyName, diagnosticMessageSink);
            }
        }

        var (hostBuilder, startup, buildHostMethod, configureMethod) =
            CreateHostBuilder(startupType, assemblyName, diagnosticMessageSink);

        return new(CreateHost(hostBuilder, startupType, startup, buildHostMethod, configureMethod),
            startupType.GetCustomAttributesData().Any(a => a.AttributeType == typeof(DisableParallelizationAttribute)));
    }

    private static DependencyInjectionContext CreateHostWithHostApplicationBuilder(Type startupType, MethodInfo methodInfo, AssemblyName? assemblyName,
        IMessageSink? diagnosticMessageSink)
    {
        // Remove the `null` assignment when this issue resolved https://github.com/dotnet/runtime/issues/90479
        var hostApplicationBuilder = Host.CreateEmptyApplicationBuilder(null);

        if (diagnosticMessageSink != null) hostApplicationBuilder.Services.TryAddSingleton(diagnosticMessageSink);
        hostApplicationBuilder.Services.TryAddSingleton<ITestOutputHelperAccessor, TestOutputHelperAccessor>();
        hostApplicationBuilder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IXunitTestCaseRunnerWrapper, DependencyInjectionTestCaseRunnerWrapper>());
        hostApplicationBuilder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IXunitTestCaseRunnerWrapper, DependencyInjectionTheoryTestCaseRunnerWrapper>());
        hostApplicationBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { HostDefaults.ApplicationKey, (assemblyName ?? startupType.Assembly.GetName()).Name }
        });

        var configureMethod = FindMethod(startupType, nameof(Configure));
        var startupObject = methodInfo.IsStatic && configureMethod is { IsStatic: true } ? null : CreateStartup(startupType);
        methodInfo.Invoke(startupObject, [hostApplicationBuilder]);
        var host = hostApplicationBuilder.Build();
        Configure(host.Services, startupObject, configureMethod);
        return new(host, startupType.GetCustomAttributesData().Any(a => a.AttributeType == typeof(DisableParallelizationAttribute)));
    }

    public static (IHostBuilder, object?, MethodInfo?, MethodInfo?) CreateHostBuilder(Type startupType,
        AssemblyName? assemblyName, IMessageSink? diagnosticMessageSink)
    {
        var createHostBuilderMethod = FindMethod(startupType, nameof(CreateHostBuilder), typeof(IHostBuilder));
        var configureHostMethod = FindMethod(startupType, nameof(ConfigureHost));
        var configureServicesMethod = FindMethod(startupType, nameof(ConfigureServices));
        var configureMethod = FindMethod(startupType, nameof(Configure));
        var buildHostMethod = FindMethod(startupType, nameof(BuildHost), typeof(IHost));

        var startup = createHostBuilderMethod is { IsStatic: false } ||
            configureHostMethod is { IsStatic: false } ||
            configureServicesMethod is { IsStatic: false } ||
            buildHostMethod is { IsStatic: false } ||
            configureMethod is { IsStatic: false }
                ? CreateStartup(startupType)
                : null;

        var hostBuilder = CreateHostBuilder(assemblyName, startup, startupType, createHostBuilderMethod) ??
            new HostBuilder();

        hostBuilder.ConfigureServices(services =>
        {
            if (diagnosticMessageSink != null) services.TryAddSingleton(diagnosticMessageSink);
            services.TryAddSingleton<ITestOutputHelperAccessor, TestOutputHelperAccessor>();

            services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IXunitTestCaseRunnerWrapper, DependencyInjectionTestCaseRunnerWrapper>());

            services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IXunitTestCaseRunnerWrapper, DependencyInjectionTheoryTestCaseRunnerWrapper>());
        });

        hostBuilder.ConfigureHostConfiguration(builder => builder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { HostDefaults.ApplicationKey, (assemblyName ?? startupType.Assembly.GetName()).Name }
        }));

        ConfigureHost(hostBuilder, startup, startupType, configureHostMethod);

        ConfigureServices(hostBuilder, startup, startupType, configureServicesMethod);

        return (hostBuilder, startup, buildHostMethod, configureMethod);
    }

    public static IHost CreateHost(IHostBuilder hostBuilder, Type startupType, object? startup,
        MethodInfo? buildHostMethod, MethodInfo? configureMethod)
    {
        var host = BuildHost(hostBuilder, startup, startupType, buildHostMethod) ?? hostBuilder.Build();

        Configure(host.Services, startup, configureMethod);

        return host;
    }

    public static Type? GetStartupType(AssemblyName assemblyName)
    {
        var assembly = Assembly.Load(assemblyName);
        var attr = assembly.GetCustomAttribute<StartupTypeAttribute>();

        if (attr == null) return assembly.GetType($"{assemblyName.Name}.Startup");

        if (attr.AssemblyName != null) assembly = Assembly.Load(attr.AssemblyName);

        return assembly.GetType(attr.TypeName);
    }

    public static object? CreateStartup(Type startupType)
    {
        if (startupType == null) throw new ArgumentNullException(nameof(startupType));

        if (startupType is { IsAbstract: true, IsSealed: true }) return null;

        var ctors = startupType.GetConstructors();
        if (ctors.Length != 1 || ctors[0].GetParameters().Length != 0)
            throw new InvalidOperationException(
                $"'{startupType.FullName}' must have a single parameterless public constructor.");

        return Activator.CreateInstance(startupType);
    }

    public static IHostBuilder? CreateHostBuilder(AssemblyName? assemblyName, object? startup, Type startupType,
        MethodInfo? method)
    {
        if (method == null) return null;

        var parameters = method.GetParameters();
        if (parameters.Length == 0)
            return (IHostBuilder?)method.Invoke(method.IsStatic ? null : startup, []);

        if (parameters.Length > 1 || parameters[0].ParameterType != typeof(AssemblyName))
            throw new InvalidOperationException(
                $"The '{method.Name}' method of startup type '{startupType.FullName}' must parameterless or have the single 'AssemblyName' parameter.");

        if (assemblyName == null)
            throw new InvalidOperationException(
                $"The '{method.Name}' method of startup type '{startupType.FullName}' must parameterless when use XunitWebApplicationFactory.");

        return (IHostBuilder?)method.Invoke(method.IsStatic ? null : startup, [assemblyName]);
    }

    public static void ConfigureHost(IHostBuilder builder, object? startup, Type startupType, MethodInfo? method)
    {
        if (method == null) return;

        var parameters = method.GetParameters();
        if (parameters.Length != 1 || parameters[0].ParameterType != typeof(IHostBuilder))
            throw new InvalidOperationException(
                $"The '{method.Name}' method of startup type '{startupType.FullName}' must have the single 'IHostBuilder' parameter.");

        method.Invoke(method.IsStatic ? null : startup, [builder]);
    }

    public static void ConfigureServices(IHostBuilder builder, object? startup, Type startupType, MethodInfo? method)
    {
        if (method == null) return;

        var parameters = method.GetParameters();
        builder.ConfigureServices(parameters.Length switch
        {
            1 when parameters[0].ParameterType == typeof(IServiceCollection) =>
                (_, services) => method.Invoke(method.IsStatic ? null : startup, [services]),
            2 when parameters[0].ParameterType == typeof(IServiceCollection) &&
                parameters[1].ParameterType == typeof(HostBuilderContext) =>
                (context, services) =>
                    method.Invoke(method.IsStatic ? null : startup, [services, context]),
            2 when parameters[1].ParameterType == typeof(IServiceCollection) &&
                parameters[0].ParameterType == typeof(HostBuilderContext) =>
                (context, services) =>
                    method.Invoke(method.IsStatic ? null : startup, [context, services]),
            _ => throw new InvalidOperationException(
                $"The '{method.Name}' method in the type '{startupType.FullName}' must have a 'IServiceCollection' parameter and optional 'HostBuilderContext' parameter.")
        });
    }

    // Not allow async Configure method
    public static void Configure(IServiceProvider provider, object? startup, MethodInfo? method)
    {
        if (method == null) return;

        using var scope = provider.CreateScope();

        method.Invoke(method.IsStatic ? null : startup,
            method.GetParameters().Select(scope.ServiceProvider.GetRequiredService).ToArray());
    }

    public static IHost? BuildHost(IHostBuilder hostBuilder, object? startup, Type startupType, MethodInfo? method)
    {
        if (method == null) return null;

        if (!typeof(IHost).IsAssignableFrom(method.ReturnType))
            throw new InvalidOperationException(
                $"The '{method.Name}' method in the type '{startupType.FullName}' return type must assignable to '{typeof(IHost)}'.");

        var parameters = method.GetParameters();
        if (parameters.Length != 1 || parameters[0].ParameterType != typeof(IHostBuilder))
            throw new InvalidOperationException(
                $"The '{method.Name}' method of startup type '{startupType.FullName}' must have the single 'IHostBuilder' parameter.");

        return (IHost?)method.Invoke(method.IsStatic ? null : startup, [hostBuilder]);
    }

    public static MethodInfo? FindMethod(Type startupType, string methodName) =>
        FindMethod(startupType, methodName, typeof(void));

    public static MethodInfo? FindMethod(Type startupType, string methodName, Type returnType)
    {
        var selectedMethods = startupType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(method => method.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase)).ToList();

        if (selectedMethods.Count > 1)
            throw new InvalidOperationException(
                $"Having multiple overloads of method '{methodName}' is not supported.");

        var methodInfo = selectedMethods.FirstOrDefault();
        if (methodInfo == null) return methodInfo;

        if (returnType == typeof(void))
        {
            if (methodInfo.ReturnType != returnType)
                throw new InvalidOperationException(
                    $"The '{methodInfo.Name}' method in the type '{startupType.FullName}' must have no return type.");
        }
        else if (!returnType.IsAssignableFrom(methodInfo.ReturnType))
            throw new InvalidOperationException(
                $"The '{methodInfo.Name}' method in the type '{startupType.FullName}' return type must assignable to '{returnType}'.");

        return methodInfo;
    }
}
