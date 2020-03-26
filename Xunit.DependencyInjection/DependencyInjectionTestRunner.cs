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
        private readonly IReadOnlyDictionary<int, Type> _fromServices;

        public DependencyInjectionTestRunner(IServiceProvider provider, ITest test, IMessageBus messageBus,
            IReadOnlyDictionary<int, Type> fromServices,
            Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments,
            string skipReason, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
            ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
            : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments,
                skipReason, beforeAfterAttributes, aggregator, cancellationTokenSource)
        {
            _provider = provider;
            _fromServices = fromServices;
        }

        /// <inheritdoc />
        protected override async Task<Tuple<decimal, string>> InvokeTestAsync(ExceptionAggregator aggregator)
        {
            var testOutputHelper = _provider.GetRequiredService<ITestOutputHelperAccessor>().Output as TestOutputHelper;
            testOutputHelper?.Initialize(MessageBus, Test);

            var raw = new Dictionary<int, object>();
            foreach (var kv in _fromServices)
            {
                raw[kv.Key] = TestMethodArguments[kv.Key];

                TestMethodArguments[kv.Key] = kv.Value == typeof(ITestOutputHelper)
                    ? _provider.GetRequiredService<ITestOutputHelperAccessor>().Output
                    : _provider.GetService(kv.Value);
            }

            var item = await InvokeTestMethodAsync(aggregator);

            foreach (var kv in raw)
                TestMethodArguments[kv.Key] = kv.Value;

            var output = string.Empty;
            if (testOutputHelper != null)
            {
                output = testOutputHelper.Output;
                testOutputHelper.Uninitialize();
            }
            return Tuple.Create(item, output);
        }

        /// <inheritdoc />
        protected override Task<decimal> InvokeTestMethodAsync(ExceptionAggregator aggregator) =>
            new DependencyInjectionTestInvoker(_provider, Test, MessageBus, TestClass,
                    ConstructorArguments, TestMethod, TestMethodArguments, BeforeAfterAttributes, aggregator, CancellationTokenSource)
                .RunAsync();
    }
}
