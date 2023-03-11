namespace Xunit.DependencyInjection;

public interface IAsyncExceptionFilter
{
    Exception Process(Exception exception);
}