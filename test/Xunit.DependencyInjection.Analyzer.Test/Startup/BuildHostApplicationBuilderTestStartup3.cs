using Microsoft.Extensions.Hosting;

namespace Xunit.DependencyInjection.Test.Analyzer.Startup;

public class Startup
{
    public IHost BuildHostApplicationBuilder(string invalidParam, int anotherParam) => null;
}