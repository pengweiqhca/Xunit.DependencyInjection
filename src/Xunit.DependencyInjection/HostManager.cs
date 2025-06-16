namespace Xunit.DependencyInjection;

internal sealed class HostManager(Assembly assembly, IMessageSink diagnosticMessageSink)
    : IHostedService, IAsyncDisposable
{
    private readonly Dictionary<Type, DependencyInjectionBuildContext> _hostMap = [];
    private readonly List<DependencyInjectionBuildContext> _hosts = [];

    private Type? _defaultStartupType;
    private DependencyInjectionBuildContext? _defaultHost;

    public DependencyInjectionBuildContext BuildDefaultHost()
    {
        _defaultStartupType = StartupLoader.GetStartupType(assembly);

        var value = _defaultStartupType == null
            ? StartupLoader.CreateEmptyStartup(assembly.GetName(), diagnosticMessageSink)
            : StartupLoader.CreateHost(_defaultStartupType, assembly, diagnosticMessageSink);

        _hosts.Add(value);

        return _defaultHost = value;
    }

    public DependencyInjectionBuildContext? GetContext(Type type)
    {
        var startupType = FindStartup(type, out var shared);
        if (startupType == null) return _defaultHost ?? throw MissingDefaultHost("Default startup is required.");

        if (shared)
        {
            if (_hostMap.TryGetValue(startupType, out var startup)) return startup;

            if (startupType == _defaultStartupType) return _hostMap[startupType] = _defaultHost!;
        }

        var host = StartupLoader.CreateHost(startupType, assembly, diagnosticMessageSink);

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
            if (index > 0) ns = ns[..index];
            else break;
        }

        return null;
    }

    public Task StartAsync(CancellationToken cancellationToken) =>
        Task.WhenAll(_hosts.Select(async x =>
        {
            try
            {
                await x.Host.StartAsync(cancellationToken);
            }
            catch
            {
                try
                {
                    x.Host.Services.GetService<IServiceProvider>();
                }
                catch (ObjectDisposedException)
                {
                    x.Disposed = true;
                }

                throw;
            }
        }));

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _hosts.Reverse();

        return Task.WhenAll(_hosts.Select(x => x.Host.StopAsync(cancellationToken)));
    }

    public ValueTask DisposeAsync() => new(Task.WhenAll(_hosts.Select(x => x.Host).Select(DisposeAsync)));

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
