using Xunit.DependencyInjection.Test.CollectionFixture;

namespace Xunit.DependencyInjection.Test.ClassFixture;

[CollectionDefinition(nameof(DisposableFixturesUnderTestCollection))]
public class DisposableFixturesUnderTestCollection : ICollectionFixture<CollectionFixtureWithDependency>;

[CollectionDefinition(nameof(MonitorDisposableFixturesCollection))]
public class MonitorDisposableFixturesCollection : ICollectionFixture<CollectionFixtureWithDependency>;

[Collection(nameof(DisposableFixturesUnderTestCollection))]
public class DisposableFixtureUnderTest : IClassFixture<FixtureWithDisposableDependency>
{
    private readonly FixtureWithDisposableDependency _fixture;
    public static FixtureWithDisposableDependency? Fixture;
    private readonly IDependencyWithManagedLifetime _dependency;
    public static IDependencyWithManagedLifetime? Dependency;

    public DisposableFixtureUnderTest(FixtureWithDisposableDependency fixture, IDependencyWithManagedLifetime dependency)
    {
        _fixture = fixture;
        Fixture = fixture;
        _dependency = dependency;
        Dependency = dependency;
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
    public void FixtureIsDisposed() => Assert.True(DisposableFixtureUnderTest.Fixture?.IsDisposed);

    [Fact]
    public void FixtureDependencyIsDisposed() =>
        Assert.True(DisposableFixtureUnderTest.Fixture?.Dependency?.IsDisposed);

    [Fact]
    public void ClassDependencyIsDisposed() => Assert.True(DisposableFixtureUnderTest.Dependency?.IsDisposed);
}
