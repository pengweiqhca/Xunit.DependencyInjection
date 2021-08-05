using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
