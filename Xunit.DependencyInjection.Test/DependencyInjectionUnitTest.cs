using Microsoft.Extensions.Configuration;

namespace Xunit.DependencyInjection.Test
{
    public class DependencyInjectionUnitTest
    {
        private readonly IDependency _d;

        public DependencyInjectionUnitTest(IDependency d) => _d = d;

        [Fact]
        public void Test1()
        {
            _d.Value++;

            Assert.True(_d.Value == 1 || _d.Value == 2);
        }

        [Fact]
        public void Test2()
        {
            _d.Value++;

            Assert.True(_d.Value == 1 || _d.Value == 2);
        }

        [Fact]
        public void Test3()
        {
            _d.TestWriteLine(100);
        }
    }
}
