using Autofac.Extensions.DependencyInjection;
using System.Diagnostics;
using Xunit.DependencyInjection.Demystifier;

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
            .AddScoped<IDependency, DependencyClass>()
            .AddScoped<IDependencyWithManagedLifetime, DependencyWithManagedLifetime>()
            .AddScoped<BeforeAfterTest, TestBeforeAfterTest>()
            .AddHostedService<HostServiceTest>()
            .AddSkippableFactSupport()
            .AddStaFactSupport()
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

        var listener = new ActivityListener();

        listener.ShouldListenTo += _ => true;
        listener.Sample += delegate { return ActivitySamplingResult.AllDataAndRecorded; };

        ActivitySource.AddActivityListener(listener);
    }
}
