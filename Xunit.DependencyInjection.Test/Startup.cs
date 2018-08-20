using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.DependencyInjection.Logging;

[assembly: TestFramework("Xunit.DependencyInjection.Test.Startup", "Xunit.DependencyInjection.Test")]
namespace Xunit.DependencyInjection.Test
{
    public class Startup : DependencyInjectionTestFramework
    {
        public Startup(IMessageSink messageSink) : base(messageSink) { }

        protected override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Information)).AddScoped<IDependency, DependencyClass>();

            return services.BuildServiceProvider();
        }

        protected override void Configure(IServiceProvider provider)
        {
            provider.GetRequiredService<ILoggerFactory>()
                .AddProvider(new XunitTestOutputLoggerProvider(provider.GetRequiredService<ITestOutputHelperAccessor>()));
        }
    }
}
