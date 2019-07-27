using Microsoft.Extensions.Hosting;
#if NETCOREAPP3_0
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostEnvironment;
#endif

namespace Xunit.DependencyInjection.Test
{
    public class HostTest
    {
        private readonly IHostingEnvironment _environment;

        public HostTest(IHostingEnvironment environment) => _environment = environment;

        [Fact]
        public void ApplicationNameTest() => Assert.Equal(typeof(HostTest).Assembly.GetName().Name, _environment.ApplicationName);
    }
}
