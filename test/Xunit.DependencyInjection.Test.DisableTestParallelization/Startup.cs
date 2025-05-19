using Microsoft.Extensions.DependencyInjection;
using Xunit.v3;

namespace Xunit.DependencyInjection.Test.Parallelization;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) =>
        services.AddSingleton<MonitorMaxParallelThreads>()
        .AddSingleton<ITestCollectionOrderer, RunMonitorCollectionLastOrderer>();
}
