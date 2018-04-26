using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

[assembly: TestFramework("Xunit.DependencyInjection.Test.ConfigureTestFramework", "Xunit.DependencyInjection.Test")]
namespace Xunit.DependencyInjection.Test
{
    public class ConfigureTestFramework : DependencyInjectionTestFramework
    {
        public ConfigureTestFramework(IMessageSink messageSink) : base(messageSink) { }

        protected override void Configuration(IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.json");
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDependency, DependencyClass>();
        }
    }
}
