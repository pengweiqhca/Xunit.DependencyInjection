using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Demystifier;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DemystifyExtensions
{
    public static IServiceCollection UseDemystifyExceptionFilter(this IServiceCollection services) =>
        services.AddSingleton<IAsyncExceptionFilter, DemystifyExceptionFilter>();
}