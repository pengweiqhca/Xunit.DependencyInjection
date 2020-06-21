namespace Xunit.DependencyInjection.Test
{
    public class ClassFixtureTest : IClassFixture<FixtureWithDependency>
    {
        private readonly FixtureWithDependency _fixture;

        public ClassFixtureTest(FixtureWithDependency fixture)
        {
            _fixture = fixture;
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

        [Fact(Skip="Implement test ordering")]
        public void SameClassFixtureDependencyInstance1()
        {
            Assert.Equal(0, _fixture.Dependency.Value);
            _fixture.Dependency.Value = 5555;
        }

        [Fact(Skip="Implement test ordering")]
        public void SameClassFixtureDependencyInstance0()
        {
            Assert.Equal(5555, _fixture.Dependency.Value);
        }
    }
}
