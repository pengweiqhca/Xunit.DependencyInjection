using Microsoft.Extensions.Logging;

namespace Xunit.DependencyInjection.Test
{
    public class XunitTestOutputLoggerTest
    {
        private readonly ILogger<XunitTestOutputLoggerTest> _logger;

        public XunitTestOutputLoggerTest(ILogger<XunitTestOutputLoggerTest> logger) => _logger = logger;

        [Fact]
        public void Test()
        {
            _logger.LogInformation("LogInformation");
            _logger.LogError("LogError");
        }
    }
}
