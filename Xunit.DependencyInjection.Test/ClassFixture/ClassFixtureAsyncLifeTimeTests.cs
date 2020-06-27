namespace Xunit.DependencyInjection.Test.ClassFixture
{
    [CollectionDefinition(nameof(ClassFixtureAsyncLifetimeTestsUnderTestCollection))]
    public class ClassFixtureAsyncLifetimeTestsUnderTestCollection : ICollectionFixture<CollectionFixtureWithDependency>
    {
    }

    [CollectionDefinition(nameof(MonitorlassFixtureAsyncLifetimeCollection))]
    public class MonitorlassFixtureAsyncLifetimeCollection : ICollectionFixture<CollectionFixtureWithDependency>
    {
    }

    [Collection(nameof(ClassFixtureAsyncLifetimeTestsUnderTestCollection))]
    public class ClassFixtureAsyncLifetimeTestsUnderTest : IClassFixture<AsyncLifetimeFixtureWithDisposableDependency>
    {
        private readonly AsyncLifetimeFixtureWithDisposableDependency _fixture;

        public static AsyncLifetimeFixtureWithDisposableDependency? s_fixture;

        public ClassFixtureAsyncLifetimeTestsUnderTest(AsyncLifetimeFixtureWithDisposableDependency fixture)
        {
            _fixture = fixture;
            s_fixture = fixture;
        }

        [Fact]
        public void FixtureWithDependencyIsInjected()
        {
            Assert.NotNull(_fixture);
        }

        [Fact]
        public void FixtureIsAsyncInitialised()
        {
            Assert.Single(_fixture.Journal, j => j == nameof(IAsyncLifetime.InitializeAsync));
        }
    }

    [Collection(nameof(MonitorlassFixtureAsyncLifetimeCollection))]
    public class ClassFixtureAsyncLifeTimeTests
    {
        [Fact]
        public void FixtureWasInitialisedAndDisposedInOrder()
        {
            Assert.Equal(nameof(IAsyncLifetime.InitializeAsync), ClassFixtureAsyncLifetimeTestsUnderTest.s_fixture?.Journal[0]);
            Assert.Equal(nameof(IAsyncLifetime.DisposeAsync), ClassFixtureAsyncLifetimeTestsUnderTest.s_fixture?.Journal[1]);
        }

        [Fact]
        public void FixtureDependencyIsDisposed()
        {
            Assert.True(ClassFixtureAsyncLifetimeTestsUnderTest.s_fixture?.Dependency.IsDisposed);
        }
    }
}
