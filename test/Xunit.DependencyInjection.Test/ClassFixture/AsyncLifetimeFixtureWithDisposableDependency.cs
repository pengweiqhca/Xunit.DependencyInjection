using Xunit.DependencyInjection.Test.CollectionFixture;

namespace Xunit.DependencyInjection.Test.ClassFixture;

public class AsyncLifetimeFixtureWithDisposableDependency(
    IDependencyWithManagedLifetime dependency,
    CollectionFixtureWithDependency collectionFixtureWithDependency) : IAsyncLifetime
{
    public IDependencyWithManagedLifetime Dependency { get; } = dependency;

    public CollectionFixtureWithDependency CollectionFixtureWithDependency { get; } = collectionFixtureWithDependency;

    public IList<string> Journal { get; } = new List<string>();

    public ValueTask InitializeAsync()
    {
        Journal.Add(nameof(InitializeAsync));

        return default;
    }

    public ValueTask DisposeAsync()
    {
        Journal.Add(nameof(DisposeAsync));

        return default;
    }
}
