using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Xunit.DependencyInjection.Test
{
    public class TestServerTest
    {
        public static string Key { get; } = Guid.NewGuid().ToString("N");

        private readonly HttpClient _httpClient;

        public TestServerTest(IServer server) => _httpClient = ((TestServer)server).CreateClient();

        [Fact]
        public async Task HttpTest()
        {
            using var response = await _httpClient.GetAsync("/").ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            Assert.Equal(Key, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }
    }
}
