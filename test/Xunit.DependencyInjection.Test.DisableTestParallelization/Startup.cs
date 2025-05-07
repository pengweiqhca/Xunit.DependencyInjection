using Microsoft.Extensions.DependencyInjection;

namespace Xunit.DependencyInjection.Test.Parallelization;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) =>
        services.AddSingleton<MaxParallelThreadsMonitor>()
        .AddSingleton<ITestCollectionOrderer, RunMonitorCollectionLastOrderer>();
}
