using MinimalApiSample;

namespace MinimalAppTest;

public class ApiTest
{
    private readonly HttpClient _httpClient;
    private readonly IRandomService _randomService;

    public ApiTest(HttpClient httpClient, IRandomService randomService)
    {
        _httpClient = httpClient;
        _randomService = randomService;
    }

    [Fact]
    public async Task ResponseTest()
    {
        var responseText = await _httpClient.GetStringAsync("/").ConfigureAwait(false);
        Assert.Equal("Hello world", responseText);
    }

    [Fact]
    public void RandomTest()
    {
        var num = _randomService.Get();
        Assert.True(num < 10);
    }
}
