using Microsoft.Extensions.Configuration;

namespace Xunit.DependencyInjection.Test
{
    public class ContentRootTest
    {
        private readonly IConfiguration _configuration;

        public ContentRootTest(IConfiguration configuration) => _configuration = configuration;

        [Fact]
        public void CanReadAppSettings()
        {
            Assert.Equal("testValue", _configuration["testKey"]);
        }
    }
}
