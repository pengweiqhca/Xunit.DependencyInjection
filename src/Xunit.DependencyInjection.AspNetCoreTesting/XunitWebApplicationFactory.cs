using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Xunit.DependencyInjection.AspNetCoreTesting;

public class XunitWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override IHostBuilder? CreateHostBuilder()
    {
        var (hostBuilder, startup, buildHostMethod, configureMethod) =
            StartupLoader.CreateHostBuilder(typeof(TStartup), null, null);

        hostBuilder.UseEnvironment(Environments.Development);

        return new XunitHostBuilder(hostBuilder, startup, buildHostMethod, configureMethod);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        if (builder is not XunitHostBuilder xunitHostBuilder) throw new InvalidOperationException();

        var host = StartupLoader.CreateHost(xunitHostBuilder.HostBuilder, typeof(TStartup), xunitHostBuilder.Startup,
            xunitHostBuilder.BuildHostMethod, xunitHostBuilder.ConfigureMethod);

        host.Start();

        return host;
    }

    private sealed class XunitHostBuilder : IHostBuilder
    {
        public IHostBuilder HostBuilder { get; }

        public object? Startup { get; }

        public MethodInfo? BuildHostMethod { get; }

        public MethodInfo? ConfigureMethod { get; }

        public XunitHostBuilder(IHostBuilder hostBuilder, object? startup, MethodInfo? buildHostMethod,
            MethodInfo? configureMethod)
        {
            HostBuilder = hostBuilder;
            Startup = startup;
            BuildHostMethod = buildHostMethod;
            ConfigureMethod = configureMethod;
        }

        public IHost Build() => HostBuilder.Build();

        public IHostBuilder ConfigureAppConfiguration(
            Action<HostBuilderContext, IConfigurationBuilder> configureDelegate) =>
            HostBuilder.ConfigureAppConfiguration(configureDelegate);

        public IHostBuilder ConfigureContainer<TContainerBuilder>(
            Action<HostBuilderContext, TContainerBuilder> configureDelegate) =>
            HostBuilder.ConfigureContainer(configureDelegate);

        public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate) =>
            HostBuilder.ConfigureHostConfiguration(configureDelegate);

        public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate) =>
            HostBuilder.ConfigureServices(configureDelegate);

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
            IServiceProviderFactory<TContainerBuilder> factory) where TContainerBuilder : notnull =>
            HostBuilder.UseServiceProviderFactory(factory);

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
            Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
            where TContainerBuilder : notnull =>
            HostBuilder.UseServiceProviderFactory(factory);

        public IDictionary<object, object> Properties => HostBuilder.Properties;
    }
}
