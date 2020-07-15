using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xunit.DependencyInjection.Test.ClassFixture
{
    public class AsyncLifetimeFixtureWithDisposableDependency : IAsyncLifetime
    {
        public AsyncLifetimeFixtureWithDisposableDependency(IDependencyWithManagedLifetime dependency) => Dependency = dependency;

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

            return  Task.CompletedTask;
        }
    }
}
