namespace Xunit.DependencyInjection.Test
{
    [TestCaseOrderer("Xunit.DependencyInjection.Test.TestCaseByMethodNameOrderer", "Xunit.DependencyInjection.Test")]
    public class ClassFixtureAndTestClassDependencyTests : IClassFixture<FixtureWithDependency>
    {
        private readonly FixtureWithDependency _fixture;
        private readonly IDependency _dependency;

        public ClassFixtureAndTestClassDependencyTests(FixtureWithDependency fixture, IDependency dependency)
        {
            _fixture = fixture;
            _dependency = dependency;
        }

        [Fact]
        public void FixtureWithDependencyIsInjected()
        {
            Assert.NotNull(_fixture);
        }

        [Fact]
        public void ClassFixtureContainsInjectedDependency()
        {
            Assert.IsType<DependencyClass>(_fixture.Dependency);
        }

        [Fact]
        public void TestCaseDependencyIsInjected()
        {
            Assert.NotNull(_dependency);
            Assert.Equal(0, _dependency.Value);
        }

        [Fact]
        public void TestCaseDependencyInstanceIsDifferentToFixtureDependencyInstance()
        {
            Assert.NotSame(_dependency, _fixture.Dependency);
        }

        [Fact]
        public void FixtureIsSharedClassDependencyIsNot_1()
        {
            Assert.Equal(0, _dependency.Value);
            Assert.Equal(0, _fixture.Dependency.Value);

            _dependency.Value++;
            _fixture.Dependency.Value++;
        }

        [Fact]
        public void FixtureIsSharedClassDependencyIsNot_2()
        {
            Assert.Equal(0, _dependency.Value);
            Assert.Equal(1, _fixture.Dependency.Value);
        }
    }
}
