namespace Xunit.DependencyInjection.Test.ClassFixture;

public class FixtureWithDisposableDependency(IDependencyWithManagedLifetime dependency) : IDisposable
{
    public IDependencyWithManagedLifetime Dependency { get; } = dependency;

    public bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
}