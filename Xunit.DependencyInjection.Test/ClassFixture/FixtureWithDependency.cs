namespace Xunit.DependencyInjection.Test
{
    public class FixtureWithDependency
    {
        private readonly IDependency _dependency;

        public FixtureWithDependency(IDependency dependency)
        {
            _dependency = dependency;
        }

        public IDependency Dependency => _dependency;
    }
}
