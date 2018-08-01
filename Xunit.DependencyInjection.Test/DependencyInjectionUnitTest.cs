using Microsoft.Extensions.Configuration;

namespace Xunit.DependencyInjection.Test
{
    public class DependencyInjectionUnitTest
    {
        private readonly IDependency _d;
        private readonly IConfiguration _configuration;

        public DependencyInjectionUnitTest(IDependency d, IConfiguration configuration)
        {
            _d = d;
            _configuration = configuration;
        }

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
            Assert.Equal("value", _configuration["key"]);
        }

        [Fact]
        public void Test4()
        {
            _d.TestWriteLine(100);
        }
    }
}
