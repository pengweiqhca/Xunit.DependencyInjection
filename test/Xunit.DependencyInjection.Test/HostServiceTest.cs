namespace Xunit.DependencyInjection.Test;

public class HostServiceTest : IHostedService
{
    private static bool HostedServiceInvoked { get; set; }

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        HostedServiceInvoked = true;

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [Fact]
    public void Test() => Assert.True(HostedServiceInvoked);
}
