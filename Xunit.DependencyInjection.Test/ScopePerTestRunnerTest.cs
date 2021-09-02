namespace Xunit.DependencyInjection.Test
{
    public class ScopePerTestRunnerTest
    {
        private readonly IDependency _d;

        public ScopePerTestRunnerTest(IDependency d) => _d = d;

        [Fact]
        public void Test1() => Assert.Equal(0, _d.Value++);

        [Fact]
        public void Test2() => Assert.Equal(0, _d.Value++);

        [Fact]
        public void Test3() => _d.TestWriteLine(100);

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Test4(int _) => Assert.Equal(0, _d.Value++);
    }
}
