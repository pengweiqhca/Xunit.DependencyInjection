using Xunit.DependencyInjection.Test.CollectionFixture;

namespace Xunit.DependencyInjection.Test.ClassFixture;

public class AsyncLifetimeFixtureWithDisposableDependency : IAsyncLifetime
{
    public AsyncLifetimeFixtureWithDisposableDependency(IDependencyWithManagedLifetime dependency, CollectionFixtureWithDependency _) =>
        Dependency = dependency;

    public IDependencyWithManagedLifetime Dependency { get; }

    public IList<string> Journal { get; } = new List<string>();

    public Task InitializeAsync()
    {
        Journal.Add(nameof(InitializeAsync));

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Journal.Add(nameof(DisposeAsync));

        return Task.CompletedTask;
    }
}
