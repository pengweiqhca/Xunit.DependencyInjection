using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit.DependencyInjection.FsCheck;

// ReSharper disable once CheckNamespace
namespace Xunit.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFsCheckSupport()
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IXunitTestCaseRunnerWrapper, PropertyTestCaseRunnerAdapter>());

            return services;
        }
    }
}
