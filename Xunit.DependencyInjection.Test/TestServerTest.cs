using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Xunit.DependencyInjection.Test
{
    public class TestServerTest
    {
        private readonly HttpClient _httpClient;

        public TestServerTest(HttpClient httpClient) => _httpClient = httpClient;
#if NETCOREAPP3_1
        [Fact]
        public async Task HttpTest()
        {
            using var response = await _httpClient.GetAsync("/").ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            Assert.True(Math.Abs(long.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false)) - DateTimeOffset.Now.ToUnixTimeMilliseconds()) < 20);
        }
#endif
    }
}
