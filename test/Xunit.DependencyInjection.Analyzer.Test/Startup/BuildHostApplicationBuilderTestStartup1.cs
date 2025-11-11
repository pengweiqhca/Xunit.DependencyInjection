using Microsoft.Extensions.Hosting;

namespace TestProject
{
    public class Startup
    {
        public IHost BuildHostApplicationBuilder(HostApplicationBuilder hostApplicationBuilder) =>
            hostApplicationBuilder.Build();
    }
}
