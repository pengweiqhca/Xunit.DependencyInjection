using Microsoft.Extensions.DependencyInjection;

namespace Xunit.DependencyInjection.Test.CollectionPerMethod;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) =>
        services.AddTransient<IDependency, Dependency>();
}
