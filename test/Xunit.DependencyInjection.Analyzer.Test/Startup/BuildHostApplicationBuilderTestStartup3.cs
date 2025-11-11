using Microsoft.Extensions.Hosting;

namespace TestProject
{
    public class Startup
    {
        public IHost BuildHostApplicationBuilder(string invalidParam, int anotherParam) => null;
    }
}
