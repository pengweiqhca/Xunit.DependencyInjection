using Microsoft.Extensions.Hosting;

namespace Xunit.DependencyInjection.Test.Analyzer.Startup;

public class Startup
{
    public object BuildHostApplicationBuilder(HostApplicationBuilder hostApplicationBuilder) => hostApplicationBuilder.Build();
}