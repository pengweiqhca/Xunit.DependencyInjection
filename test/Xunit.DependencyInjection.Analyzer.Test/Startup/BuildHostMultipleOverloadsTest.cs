using Microsoft.Extensions.Hosting;

namespace TestProject
{
    public class Startup
    {
        public IHost BuildHost(IHostBuilder builder) => builder.Build();
        
        // This should trigger MultipleOverloads warning with the fix
        public IHost BuildHost(IHostBuilder builder, string extra) => builder.Build();
    }
}