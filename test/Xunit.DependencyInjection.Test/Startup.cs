using Autofac.Extensions.DependencyInjection;
using Xunit.DependencyInjection.Demystifier;
using Xunit.v3;

namespace Xunit.DependencyInjection.Test;

public class Startup
{
    public static int Counter { get; set; }

    public Startup() => Counter++;

    public void ConfigureHost(IHostBuilder hostBuilder) =>
        hostBuilder.ConfigureAppConfiguration(lb => lb.AddJsonFile("appsettings.json", false, true))
            .UseServiceProviderFactory(new AutofacServiceProviderFactory());

    public void ConfigureServices(IServiceCollection services) =>
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Debug).AddXunitOutput())
            .AddKeyedSingleton<IDependency, DependencyClass>(KeyedService.AnyKey)
            .AddScoped<IDependency, DependencyClass>()
            .AddScoped<IDependencyWithManagedLifetime, DependencyWithManagedLifetime>()
            .AddScoped<BeforeAfterTest, TestBeforeAfterTest>()
            .AddHostedService<HostServiceTest>()
            .AddStaFactSupport()
            .AddSingleton<ITestCollectionOrderer, RunMonitorCollectionLastOrderer>()
            .AddSingleton<ITestClassOrderer, TestClassByOrderOrderer>()
            .AddSingleton<ITestCaseOrderer, TestCaseByMethodNameOrderer>()
            .AddKeyedScoped<IFromKeyedServicesTest, FromSmallKeyedServicesTest>("small")
            .AddKeyedScoped<IFromKeyedServicesTest, FromLargeKeyedServicesTest>("large")
            .AddXRetrySupport()
            .AddSingleton<IAsyncExceptionFilter, DemystifyExceptionFilter>();

    public void Configure(IServiceProvider provider, ITestOutputHelperAccessor accessor,
        [FromServices] IAsyncExceptionFilter filter, [FromKeyedServices("small")] IFromKeyedServicesTest test)
    {
        Assert.NotNull(accessor);
        Assert.IsType<DemystifyExceptionFilter>(filter);
        Assert.IsType<FromSmallKeyedServicesTest>(test);
    }
}
