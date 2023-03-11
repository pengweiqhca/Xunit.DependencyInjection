using Xunit.DependencyInjection;

[assembly: StartupType("TestProject.Startup2")]
namespace TestProject
{
    public class Startup2
    {
        public Startup2() { }
        public Startup2(string abc) { }
    }
}
