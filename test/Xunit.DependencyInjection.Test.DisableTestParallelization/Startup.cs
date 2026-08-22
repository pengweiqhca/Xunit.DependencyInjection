using Microsoft.Extensions.DependencyInjection;
using Xunit.v3;

#if DisableTestParallelization
[assembly: Parallelization(MaxThreads = 1, Mode = ParallelMode.None)]
#else
[assembly: Parallelization(MaxThreads = 2, Mode = ParallelMode.All)]
#endif

namespace Xunit.DependencyInjection.Test.Parallelization;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) =>
        services.AddSingleton<MonitorMaxParallelThreads>()
        .AddSingleton<ITestCollectionOrderer, RunMonitorCollectionLastOrderer>();
}
