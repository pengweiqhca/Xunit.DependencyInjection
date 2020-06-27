using System;

namespace Xunit.DependencyInjection.Test
{
    public class DependencyWithManagedLifetime : IDependencyWithManagedLifetime, IDisposable
    {
        public int Value { get; set; }

        public bool IsDisposed { get; private set; }

        public void Dispose() => IsDisposed = true;
    }
}
