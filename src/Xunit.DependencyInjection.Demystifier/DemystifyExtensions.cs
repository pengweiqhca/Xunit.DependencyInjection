using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Demystifier;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DemystifyExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection UseDemystifyExceptionFilter() =>
            services.AddSingleton<IAsyncExceptionFilter, DemystifyExceptionFilter>();
    }
}
