using Microsoft.Extensions.Hosting;
using System;

namespace TestProject
{
    public class Startup
    {
        public String ConfigureHost(IHostBuilder builder) => Test();

        private static string Test() => "";
    }
}
