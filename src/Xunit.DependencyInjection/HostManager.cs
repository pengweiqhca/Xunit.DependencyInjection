namespace Xunit.DependencyInjection;

internal sealed class HostManager(AssemblyName assemblyName, IMessageSink diagnosticMessageSink)
    : IHostedService, IDisposable
{
    private readonly Dictionary<Type, DependencyInjectionContext> _hostMap = [];
    private readonly List<IHost> _hosts = [];

    private Type? _defaultStartupType;
    private DependencyInjectionContext? _defaultHost;

    public DependencyInjectionContext? BuildDefaultHost()
    {
        _defaultStartupType = StartupLoader.GetStartupType(assemblyName);

        if (_defaultStartupType == null) return _defaultHost;

        var value = StartupLoader.CreateHost(_defaultStartupType, assemblyName, diagnosticMessageSink);

        _hosts.Add(value.Host);

        return _defaultHost = value;
    }

    public DependencyInjectionContext? GetContext(Type type)
    {
        var startupType = FindStartup(type, out var shared);
        if (startupType == null) return _defaultHost ?? throw MissingDefaultHost("Default startup is required.");

        if (shared)
        {
            if (_hostMap.TryGetValue(startupType, out var startup)) return startup;

            if (startupType == _defaultStartupType) return _hostMap[startupType] = _defaultHost!;
        }

        var host = StartupLoader.CreateHost(startupType, assemblyName, diagnosticMessageSink);

        if (shared) _hostMap[startupType] = host;

        _hosts.Add(host.Host);

        return host;
    }

    public static Exception MissingDefaultHost(string message) =>
        new InvalidOperationException(message + Environment.NewLine +
            "https://github.com/pengweiqhca/Xunit.DependencyInjection#4-default-startup");

    private static Type? FindStartup(Type testClassType, out bool shared)
    {
        shared = true;

        var attr = testClassType.GetCustomAttribute<StartupAttribute>();
        if (attr != null)
        {
            shared = attr.Shared;

            return attr.StartupType;
        }

        var type = testClassType;
        while (type != null)
        {
            var startupType = type.GetNestedType("Startup");
            if (startupType != null) return startupType;

            testClassType = type;

            type = type.DeclaringType;
        }

        var ns = testClassType.Namespace;
        while (true)
        {
            var startupTypeString = "Startup";
            if (!string.IsNullOrEmpty(ns))
                startupTypeString = ns + ".Startup";

            var startupType = testClassType.Assembly.GetType(startupTypeString);
            if (startupType != null) return startupType;

            var index = ns!.LastIndexOf('.');
            if (index > 0) ns = ns.Substring(0, index);
            else break;
        }

        return null;
    }

    public Task StartAsync(CancellationToken cancellationToken) =>
        Task.WhenAll(_hosts.Select(x => x.StartAsync(cancellationToken)));

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _hosts.Reverse();

        return Task.WhenAll(_hosts.Select(x => x.StopAsync(cancellationToken)));
    }

    //DisposalTracker not support IAsyncDisposable
    public void Dispose() => Task.WaitAll(_hosts.Select(DisposeAsync).ToArray());

    private static Task DisposeAsync(IDisposable disposable)
    {
        switch (disposable)
        {
            case IAsyncDisposable ad:
                return ad.DisposeAsync().AsTask();
            default:
                disposable.Dispose();

                return Task.CompletedTask;
        }
    }
}
