namespace Xunit.DependencyInjection;

internal sealed class HostManager : IHostedService, IDisposable
{
    private readonly Dictionary<Type, IHost> _hostMap = new Dictionary<Type, IHost>();
    private readonly List<IHost> _hosts = new List<IHost>();

    private Type? _defaultStartupType;
    private IHost? _defaultHost;
    private readonly AssemblyName _assemblyName;
    private readonly IMessageSink _diagnosticMessageSink;

    public HostManager(AssemblyName assemblyName, IMessageSink diagnosticMessageSink)
    {
        _assemblyName = assemblyName;
        _diagnosticMessageSink = diagnosticMessageSink;
    }

    public IHost? BuildDefaultHost()
    {
        _defaultStartupType = StartupLoader.GetStartupType(_assemblyName);

        if (_defaultStartupType != null)
            _hosts.Add(_defaultHost =
                StartupLoader.CreateHost(_defaultStartupType, _assemblyName, _diagnosticMessageSink));

        return _defaultHost;
    }

    public IHost GetHost(Type type)
    {
        var startupType = FindStartup(type, out var shared);
        if (startupType == null) return _defaultHost ?? throw MissingDefaultHost("Default startup is required.");

        if (shared)
        {
            if (_hostMap.TryGetValue(startupType, out var startup)) return startup;

            if (startupType == _defaultStartupType) return _hostMap[startupType] = _defaultHost!;
        }

        var host = StartupLoader.CreateHost(startupType, _assemblyName, _diagnosticMessageSink);

        if (shared) _hostMap[startupType] = host;

        _hosts.Add(host);

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
        var tasks = new Task[_hosts.Count];

        for (var index = _hosts.Count - 1; index >= 0; index--)
            tasks[index] = _hosts[index].StopAsync(cancellationToken);

        return Task.WhenAll(tasks);
    }

    //DisposalTracker not support IAsyncDisposable
    public void Dispose()
    {
        for (var index = _hosts.Count - 1; index >= 0; index--)
        {
            var task = _hosts[index].DisposeAsync();

            if (!task.IsCompletedSuccessfully)
                task.AsTask().GetAwaiter().GetResult();
        }
    }
}
