using Microsoft.Extensions.Hosting;

namespace TestProject
{
    public class Startup
    {
        public object BuildHostApplicationBuilder(HostApplicationBuilder hostApplicationBuilder) =>
            hostApplicationBuilder.Build();
    }
}
