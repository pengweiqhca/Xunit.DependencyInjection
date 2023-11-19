namespace Xunit.DependencyInjection.Test.CollectionFixture;

public class CollectionFixtureWithDependency(IDependency dependency) : IDisposable
{
    public bool IsDisposed { get; private set; }

    public IDependency Dependency { get; } = dependency;

    public void Dispose() => IsDisposed = true;
}