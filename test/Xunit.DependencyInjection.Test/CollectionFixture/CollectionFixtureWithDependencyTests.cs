namespace Xunit.DependencyInjection.Test.CollectionFixture;

[CollectionDefinition(nameof(CollectionFixtureWithDependencyTestsCollection))]
public class CollectionFixtureWithDependencyTestsCollection : ICollectionFixture<CollectionFixtureWithDependency>
{
}

[CollectionDefinition(nameof(MonitorCollectionFixtureCollection))]
public class MonitorCollectionFixtureCollection : ICollectionFixture<CollectionFixtureWithDependency>
{
}

[Collection(nameof(CollectionFixtureWithDependencyTestsCollection))]
public class CollectionFixtureWithDependencyTests_A
{
    private readonly CollectionFixtureWithDependency _fixture;
    public static CollectionFixtureWithDependency? s_fixture;

    public CollectionFixtureWithDependencyTests_A(CollectionFixtureWithDependency fixture)
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
    public void FixtureContainsInjectedDependency()
    {
        Assert.IsType<DependencyClass>(_fixture.Dependency);
    }

    [Fact]
    public void TestsInCollectionShareCollectionFixture()
    {
        if (CollectionFixtureWithDependencyTests_B.s_fixture != null) Assert.Same(CollectionFixtureWithDependencyTests_B.s_fixture, _fixture);
    }
}

[Collection(nameof(CollectionFixtureWithDependencyTestsCollection))]
public class CollectionFixtureWithDependencyTests_B
{
    private readonly CollectionFixtureWithDependency _fixture;
    public static CollectionFixtureWithDependency? s_fixture;

    public CollectionFixtureWithDependencyTests_B(CollectionFixtureWithDependency fixture)
    {
        _fixture = fixture;
        s_fixture = fixture;
    }

    [Fact]
    public void FixtureWithDependencyIsInjected()
    {
        Assert.NotNull(_fixture);
    }
}

[Collection(nameof(MonitorCollectionFixtureCollection))]
public class CollectionFixtureTestMonitor
{
    [Fact]
    public void TestClassesInTheSameCollectionReceiveTheSameFixture()
    {
        Assert.NotNull(CollectionFixtureWithDependencyTests_A.s_fixture);
        Assert.Same(CollectionFixtureWithDependencyTests_A.s_fixture, CollectionFixtureWithDependencyTests_B.s_fixture);
    }

    [Fact]
    public void FixtureIsDisposed()
    {
        Assert.True(CollectionFixtureWithDependencyTests_A.s_fixture?.IsDisposed);
    }
}