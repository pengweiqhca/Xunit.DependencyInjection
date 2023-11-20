namespace Xunit.DependencyInjection.Test;

public class ContentRootTest(IConfiguration configuration)
{
    [Fact]
    public void CanReadAppSettings() => Assert.Equal("testValue", configuration["testKey"]);
}
