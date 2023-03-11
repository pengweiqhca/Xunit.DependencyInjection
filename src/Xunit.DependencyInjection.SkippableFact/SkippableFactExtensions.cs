using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit.DependencyInjection.SkippableFact;

// ReSharper disable once CheckNamespace
namespace Xunit.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSkippableFactSupport(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IXunitTestCaseRunnerWrapper, SkippableFactTestCaseRunnerWrapper>());

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IXunitTestCaseRunnerWrapper, SkippableTheoryTestCaseRunnerWrapper>());

        return services;
    }
}