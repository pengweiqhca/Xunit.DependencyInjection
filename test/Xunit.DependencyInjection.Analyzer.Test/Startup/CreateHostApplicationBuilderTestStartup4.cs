using Microsoft.Extensions.Hosting;

namespace TestProject
{
    public class Startup
    {
        public HostApplicationBuilder CreateHostApplicationBuilder(string invalidParam, int anotherParam) => null;
    }
}
