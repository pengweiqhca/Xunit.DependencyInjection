using Microsoft.Extensions.Hosting;
using Xunit.DependencyInjection.AspNetCoreTesting;

namespace MinimalAppTest;

public class Startup
{
    public IHostBuilder CreateHostBuilder() => MinimalApiHostBuilderFactory.GetHostBuilder<Program>();
}
