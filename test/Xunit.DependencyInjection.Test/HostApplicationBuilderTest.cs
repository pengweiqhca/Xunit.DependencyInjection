namespace Xunit.DependencyInjection.Test;

public class HostApplicationBuilderTest(IConfiguration configuration, IServiceProvider serviceProvider)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    [Fact]
    public void ConfigurationTest() => Assert.Equal("World", _configuration["Hello"]);

    [Fact]
    public void ServiceTest()
    {
        var idGenerator = _serviceProvider.GetService<IIdGenerator>();
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

        public static void Configure() => Counter++;
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
