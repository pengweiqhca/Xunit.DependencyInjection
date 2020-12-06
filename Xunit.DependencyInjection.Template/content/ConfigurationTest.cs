using Microsoft.Extensions.Configuration;
using Xunit;

namespace TProject
{
    public class ConfigurationTest
    {
        private readonly IConfiguration _configuration;

        public ConfigurationTest(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Fact]
        public void ConfigurationGetTest()
        {
            Assert.NotNull(_configuration);
        }
    }
}
