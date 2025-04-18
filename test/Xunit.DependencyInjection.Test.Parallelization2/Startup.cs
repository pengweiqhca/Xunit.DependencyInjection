﻿using Microsoft.Extensions.DependencyInjection;
using Xunit.v3;

namespace Xunit.DependencyInjection.Test.Parallelization2;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) =>
        services.AddSingleton<MaxParallelThreadsMonitor>()
        .AddSingleton<ITestCollectionOrderer, RunMonitorCollectionLastOrderer>();
}
