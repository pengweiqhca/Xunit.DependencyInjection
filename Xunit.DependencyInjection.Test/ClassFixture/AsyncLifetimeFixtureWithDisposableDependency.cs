using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xunit.DependencyInjection.Test.ClassFixture
{
    public class AsyncLifetimeFixtureWithDisposableDependency : IAsyncLifetime
    {
        private readonly IDependencyWithManagedLifetime _dependency;

        public AsyncLifetimeFixtureWithDisposableDependency(IDependencyWithManagedLifetime dependency)
        {
            _dependency = dependency;
        }

        public IDependencyWithManagedLifetime Dependency => _dependency;

        public IList<string> Journal { get; } = new List<string>();

        public Task InitializeAsync()
        {
            Journal.Add(nameof(InitializeAsync));
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            Journal.Add(nameof(DisposeAsync));
            return  Task.CompletedTask;
        }
    }
}
