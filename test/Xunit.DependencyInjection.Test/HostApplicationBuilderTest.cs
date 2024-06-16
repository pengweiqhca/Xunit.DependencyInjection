namespace Xunit.DependencyInjection.Test;

public class HostApplicationBuilderTest(IConfiguration configuration, IServiceProvider serviceProvider)
{
    [Fact]
    public void ConfigurationTest() => Assert.Equal("World", configuration["Hello"]);

    [Fact]
    public void ServiceTest()
    {
        var idGenerator = serviceProvider.GetService<IIdGenerator>();
        Assert.NotNull(idGenerator);
        Assert.True(idGenerator is GuidIdGenerator);
    }

    [Fact]
    public void ConfigureTest() => Assert.Equal(1, Startup.Counter);

    public class Startup
    {
        public static int Counter { get; private set; }

        public void ConfigureHostApplicationBuilder(IHostApplicationBuilder hostApplicationBuilder)
        {
            hostApplicationBuilder.Configuration.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("Hello", "World")
            });
            hostApplicationBuilder.Services.AddSingleton<IIdGenerator, GuidIdGenerator>();
        }

        public static void Configure(IHostEnvironment environment)
        {
            Assert.NotEmpty(environment.ApplicationName);

            Counter++;
        }
    }
}

file interface IIdGenerator
{
    string NewId();
}

file sealed class GuidIdGenerator: IIdGenerator
{
    public string NewId() => Guid.NewGuid().ToString();
}
