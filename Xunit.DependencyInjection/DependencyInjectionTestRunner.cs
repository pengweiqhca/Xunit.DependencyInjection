using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestRunner : XunitTestRunner
    {
        private readonly IServiceProvider _provider;

        public DependencyInjectionTestRunner(IServiceProvider provider, ITest test, IMessageBus messageBus,
            Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments,
            string skipReason, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
            ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
            : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments,
                skipReason, beforeAfterAttributes, aggregator, cancellationTokenSource) =>
            _provider = provider;

        protected override async Task<Tuple<decimal, string>> InvokeTestAsync(ExceptionAggregator aggregator)
        {
            var testOutputHelper = _provider.GetRequiredService<ITestOutputHelperAccessor>().Output as TestOutputHelper;
            if (testOutputHelper != null)
                testOutputHelper.Initialize(MessageBus, Test);

            var item = await InvokeTestMethodAsync(aggregator);

            var output = string.Empty;
            if (testOutputHelper != null)
            {
                output = testOutputHelper.Output;
                testOutputHelper.Uninitialize();
            }
            return Tuple.Create(item, output);
        }
    }
}
