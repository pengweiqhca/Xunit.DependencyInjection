using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.DependencyInjection.Logging;

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

        protected override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDependency, DependencyClass>();

            return services.BuildServiceProvider();
        }

        protected override void Configure(IServiceProvider provider)
        {
            provider.GetRequiredService<ILoggerFactory>()
                .AddProvider(new XunitTestOutputLoggerProvider(provider.GetRequiredService<ITestOutputHelperAccessor>()));
        }
    }
}
