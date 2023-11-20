namespace Xunit.DependencyInjection.Test;

public class XunitTestOutputLoggerTest(ILogger<XunitTestOutputLoggerTest> logger)
{
    [Fact]
    public void Test()
    {
        logger.LogDebug("LogDebug");
        logger.LogInformation("LogInformation");
        logger.LogError("LogError");
    }
}
