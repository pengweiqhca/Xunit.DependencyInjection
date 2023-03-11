using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit.DependencyInjection.xRetry;

// ReSharper disable once CheckNamespace
namespace Xunit.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXRetrySupport(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IXunitTestCaseRunnerWrapper, RetryTestCaseRunnerWrapper>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IXunitTestCaseRunnerWrapper, RetryTheoryDiscoveryAtRuntimeRunnerWrapper>());

        return services;
    }
}
