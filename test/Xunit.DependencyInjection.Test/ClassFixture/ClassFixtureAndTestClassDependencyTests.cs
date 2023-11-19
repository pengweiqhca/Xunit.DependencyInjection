namespace Xunit.DependencyInjection.Test.ClassFixture;

[TestCaseOrderer("Xunit.DependencyInjection.Test." + nameof(TestCaseByMethodNameOrderer), "Xunit.DependencyInjection.Test")]
public class ClassFixtureAndTestClassDependencyTests(FixtureWithDependency fixture, IDependency dependency)
    : IClassFixture<FixtureWithDependency>
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
    public void TestCaseDependencyIsInjected()
    {
        Assert.NotNull(dependency);
        Assert.Equal(0, dependency.Value);
    }

    [Fact]
    public void TestCaseDependencyInstanceIsDifferentToFixtureDependencyInstance()
    {
        Assert.NotSame(dependency, fixture.Dependency);
    }

    [Fact]
    public void FixtureIsSharedClassDependencyIsNot_1()
    {
        Assert.Equal(0, dependency.Value);
        Assert.Equal(0, fixture.Dependency.Value);

        dependency.Value++;
        fixture.Dependency.Value++;
    }

    [Fact]
    public void FixtureIsSharedClassDependencyIsNot_2()
    {
        Assert.Equal(0, dependency.Value);
        Assert.Equal(1, fixture.Dependency.Value);
    }
}