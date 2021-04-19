using System.Reflection;

namespace TestProject
{
    public class Startup
    {
        public void CreateHostBuilder(AssemblyName name) => Test();

        private static Microsoft.Extensions.Hosting.IHostBuilder Test() => null;
    }
}
