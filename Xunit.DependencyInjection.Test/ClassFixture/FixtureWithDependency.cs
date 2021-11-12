namespace Xunit.DependencyInjection.Test.ClassFixture;

public class FixtureWithDependency
{
    public FixtureWithDependency(IDependency dependency) => Dependency = dependency;

    public IDependency Dependency { get; }
}