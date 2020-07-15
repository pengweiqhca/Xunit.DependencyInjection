using System;

namespace Xunit.DependencyInjection.Test.CollectionFixture
{
    public class CollectionFixtureWithDependency : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public CollectionFixtureWithDependency(IDependency dependency) => Dependency = dependency;

        public IDependency Dependency { get; }

        public void Dispose() => IsDisposed = true;
    }
}
