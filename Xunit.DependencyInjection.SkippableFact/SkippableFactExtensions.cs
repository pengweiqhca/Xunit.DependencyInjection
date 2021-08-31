using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit.DependencyInjection.SkippableFact;

// ReSharper disable once CheckNamespace
namespace Xunit.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSkippableFactSupport(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IXunitTestCaseRunnerWrapper, SkippableFactTestCaseRunnerWrapper>());
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IXunitTestCaseRunnerWrapper, SkippableTheoryTestCaseRunnerWrapper>());

            return services;
        }
    }
}
