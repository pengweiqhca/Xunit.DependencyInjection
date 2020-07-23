using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Xunit.DependencyInjection.Test
{
    public class HostServiceTest : IHostedService
    {
        private static bool HostedServiceInvoked { get; set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            HostedServiceInvoked = true;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        [Fact]
        public void Test() => Assert.True(HostedServiceInvoked);
    }
}
