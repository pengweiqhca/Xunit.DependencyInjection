using System;

namespace Xunit.DependencyInjection.Test
{
    public class CollectionFixtureWithDependency : IDisposable
    {
        private readonly IDependency _dependency;

        public bool IsDisposed { get; private set; }

        public CollectionFixtureWithDependency(IDependency dependency)
        {
            _dependency = dependency;
        }

        public IDependency Dependency => _dependency;

        public int Value { get; set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
