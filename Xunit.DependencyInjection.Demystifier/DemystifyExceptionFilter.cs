using System.Diagnostics;

namespace Xunit.DependencyInjection.Demystifier;

public class DemystifyExceptionFilter : IAsyncExceptionFilter
{
    public Exception Process(Exception exception) => exception.Demystify();
}