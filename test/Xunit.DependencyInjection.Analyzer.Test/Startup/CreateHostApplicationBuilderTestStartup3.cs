using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Xunit.DependencyInjection.Test.Analyzer.Startup;

public class Startup
{
    public HostApplicationBuilder CreateHostApplicationBuilder(AssemblyName assemblyName) => Host.CreateEmptyApplicationBuilder();
}