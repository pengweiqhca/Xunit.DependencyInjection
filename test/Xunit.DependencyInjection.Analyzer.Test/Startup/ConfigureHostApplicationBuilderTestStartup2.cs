using Microsoft.Extensions.Hosting;

namespace Xunit.DependencyInjection.Test.Analyzer.Startup;

public class Startup
{
    public int ConfigureHostApplicationBuilder(IHostApplicationBuilder hostApplicationBuilder) => 1;
}