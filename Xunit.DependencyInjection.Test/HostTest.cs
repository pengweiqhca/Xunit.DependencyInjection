using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Xunit.DependencyInjection.Test
{
    public class HostTest
    {
        private readonly IHostingEnvironment _environment;
        private readonly IServiceProvider _provider;

        public HostTest(IHostingEnvironment environment, IServiceProvider provider)
        {
            _environment = environment;
            _provider = provider;
        }

        [Fact]
        public void ApplicationNameTest() => Assert.Equal(typeof(HostTest).Assembly.GetName().Name, _environment.ApplicationName);

        [Fact]
        public void IsAutofac() => Assert.IsType<AutofacServiceProvider>(_provider);
    }
}
