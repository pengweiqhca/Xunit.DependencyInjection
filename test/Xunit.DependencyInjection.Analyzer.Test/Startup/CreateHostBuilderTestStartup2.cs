using System.Reflection;

namespace TestProject
{
    public class Startup
    {
        public Microsoft.Extensions.Hosting.IHostBuilder CreateHostBuilder(AssemblyName name) => Test();

        private static Microsoft.Extensions.Hosting.IHostBuilder Test() => null;
    }
}
