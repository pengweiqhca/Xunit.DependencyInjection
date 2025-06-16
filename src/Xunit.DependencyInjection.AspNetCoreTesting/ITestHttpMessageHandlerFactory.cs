namespace Xunit.DependencyInjection.AspNetCoreTesting;

public interface ITestHttpMessageHandlerFactory
{
    HttpMessageHandler CreateHandler();
}
