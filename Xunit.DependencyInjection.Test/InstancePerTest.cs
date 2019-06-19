using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace Xunit.DependencyInjection.Test
{
    public class InstancePerTest : IDisposable
    {
        private readonly IServiceScope _serviceScope;
        private readonly IDependency _d;

        public InstancePerTest(IServiceProvider provider, CancellationToken cancellationToken)
        {
            _serviceScope = provider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            _d = _serviceScope.ServiceProvider.GetRequiredService<IDependency>();

            Assert.NotEqual(CancellationToken.None, cancellationToken);
        }

        [Fact]
        public void Test1()
        {
            _d.Value++;

            Assert.Equal(1, _d.Value);
        }

        [Fact]
        public void Test2()
        {
            _d.Value++;

            Assert.Equal(1, _d.Value);
        }

        [Fact]
        public void Test3()
        {
            _d.TestWriteLine(100);
        }

        public void Dispose() => _serviceScope.Dispose();
    }
}
