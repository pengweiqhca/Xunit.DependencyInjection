namespace Xunit.DependencyInjection.Test.ClassFixture;

[TestCaseOrderer("Xunit.DependencyInjection.Test." + nameof(TestCaseByMethodNameOrderer), "Xunit.DependencyInjection.Test")]
public class ClassFixtureTest(FixtureWithDependency fixture) : IClassFixture<FixtureWithDependency>
{
    [Fact]
    public void FixtureWithDependencyIsInjected()
    {
        Assert.NotNull(fixture);
    }

    [Fact]
    public void ClassFixtureContainsInjectedDependency()
    {
        Assert.IsType<DependencyClass>(fixture.Dependency);
    }

    [Fact]
    public void SameClassFixtureDependencyInstance_1()
    {
        Assert.Equal(0, fixture.Dependency.Value);
        fixture.Dependency.Value = 5555;
    }

    [Fact]
    public void SameClassFixtureDependencyInstance_2()
    {
        Assert.Equal(5555, fixture.Dependency.Value);
    }
}