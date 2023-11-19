namespace Xunit.DependencyInjection.Test.ClassFixture;

public class FixtureWithDependency(IDependency dependency)
{
    public IDependency Dependency { get; } = dependency;
}