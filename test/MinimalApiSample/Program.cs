using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MinimalApiSample.IRandomService, MinimalApiSample.RandomService>();

builder.Services.AddControllers();

var app = builder.Build();

app.Map("/hello", () => "Hello world");
app.MapDefaultControllerRoute();

await app.RunAsync();

public class HomeController
{
    public IActionResult Index() => new ContentResult { Content = "Hello world" };
}
