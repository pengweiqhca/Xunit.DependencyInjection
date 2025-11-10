using Microsoft.Extensions.Hosting;

namespace Xunit.DependencyInjection.Test.Analyzer.Startup;

public class Startup
{
    public HostApplicationBuilder CreateHostApplicationBuilder(string invalidParam, int anotherParam) => Host.CreateEmptyApplicationBuilder();
}