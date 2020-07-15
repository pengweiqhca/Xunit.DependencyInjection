using System;

namespace Xunit.DependencyInjection.Test.ClassFixture
{
    public class FixtureWithDisposableDependency : IDisposable
    {
        public FixtureWithDisposableDependency(IDependencyWithManagedLifetime dependency) => Dependency = dependency;

        public IDependencyWithManagedLifetime Dependency { get; }

        public bool IsDisposed { get; private set; }

        public void Dispose() => IsDisposed = true;
    }
}
