using Microsoft.Extensions.Hosting;

namespace Xunit.DependencyInjection.Test.Analyzer.Startup;

public class Startup
{
    public IHost BuildHostApplicationBuilder(HostApplicationBuilder hostApplicationBuilder) => hostApplicationBuilder.Build();
}