using System;

namespace Xunit.DependencyInjection.Test.ClassFixture
{
    public class FixtureWithDisposableDependency : IDisposable
    {
        private readonly IDependencyWithManagedLifetime _dependency;

        public FixtureWithDisposableDependency(IDependencyWithManagedLifetime dependency)
        {
            _dependency = dependency;
        }

        public IDependencyWithManagedLifetime Dependency => _dependency;

        public bool IsDisposed { get; private set; }

        public void Dispose() => IsDisposed = true;
    }
}
