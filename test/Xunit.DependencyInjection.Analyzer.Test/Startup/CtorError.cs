using TestProject;
using Xunit.DependencyInjection;

[assembly: StartupType(typeof(Startup2))]
namespace TestProject
{
    public class Startup2
    {
        public Startup2(string abc) { }
    }
}
