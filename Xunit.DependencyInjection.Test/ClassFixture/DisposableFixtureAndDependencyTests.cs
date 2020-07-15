namespace Xunit.DependencyInjection.Test.ClassFixture
{
    [CollectionDefinition(nameof(DisposableFixturesUnderTestCollection))]
    public class DisposableFixturesUnderTestCollection : ICollectionFixture<CollectionFixtureWithDependency>
    {
    }

    [CollectionDefinition(nameof(MonitorDisposableFixturesCollection))]
    public class MonitorDisposableFixturesCollection : ICollectionFixture<CollectionFixtureWithDependency>
    {
    }

    [Collection(nameof(DisposableFixturesUnderTestCollection))]
    public class DisposableFixtureUnderTest : IClassFixture<FixtureWithDisposableDependency>
    {
        private readonly FixtureWithDisposableDependency _fixture;
        public static FixtureWithDisposableDependency? s_fixture;
        private readonly IDependencyWithManagedLifetime _dependency;
        public static IDependencyWithManagedLifetime? s_dependency;

        public DisposableFixtureUnderTest(FixtureWithDisposableDependency fixture, IDependencyWithManagedLifetime dependency)
        {
            _fixture = fixture;
            s_fixture = fixture;
            _dependency = dependency;
            s_dependency = dependency;
        }

        [Fact]
        public void DependenciesAreInjected()
        {
            Assert.NotNull(_fixture);
            Assert.NotNull(_dependency);
        }
    }

    [Collection(nameof(MonitorDisposableFixturesCollection))]
    public class DisposableFixtureTests
    {
        [Fact]
        public void FixtureIsDisposed()
        {
            Assert.True(DisposableFixtureUnderTest.s_fixture?.IsDisposed);
        }

        [Fact]
        public void FixtureDependencyIsDisposed()
        {
            Assert.True(DisposableFixtureUnderTest.s_fixture?.Dependency?.IsDisposed);
        }

        [Fact]
        public void ClassDependencyIsDisposed()
        {
            Assert.True(DisposableFixtureUnderTest.s_dependency?.IsDisposed);
        }
    }
}
