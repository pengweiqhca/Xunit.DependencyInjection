using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using xRetry.v3;
using Xunit.DependencyInjection.xRetry;

// ReSharper disable once CheckNamespace
namespace Xunit.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddXRetrySupport()
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IXunitTestCaseRunnerWrapper, RetryTestCaseRunnerWrapper<RetryTestCase>>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IXunitTestCaseRunnerWrapper, RetryTestCaseRunnerWrapper<RetryTheoryDiscoveryAtRuntimeCase>>());

            return services;
        }
    }
}
