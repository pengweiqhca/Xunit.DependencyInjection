using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit.DependencyInjection.StaFact;

// ReSharper disable once CheckNamespace
namespace Xunit.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStaFactSupport(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IXunitTestCaseRunnerWrapper, UITestCaseRunnerAdapter>());

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IXunitTestCaseRunnerWrapper, UITheoryTestCaseRunnerAdapter>());

        return services;
    }
}
