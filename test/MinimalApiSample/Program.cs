var builder = WebApplication.CreateBuilder();
builder.Services.AddSingleton<MinimalApiSample.IRandomService, MinimalApiSample.RandomService>();
var app = builder.Build();
app.Map("/", () => "Hello world");
await app.RunAsync().ConfigureAwait(false);

public partial class Program {}
