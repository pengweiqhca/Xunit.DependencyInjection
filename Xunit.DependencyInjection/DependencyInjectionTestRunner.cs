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
            var scope = _provider.CreateScope();

            await using var _ = scope.AsAsyncDisposable().ConfigureAwait(false);

            var testOutputHelper = _provider.GetRequiredService<ITestOutputHelperAccessor>().Output as TestOutputHelper;
            testOutputHelper?.Initialize(MessageBus, Test);

            var raw = new Dictionary<int, object>();
            foreach (var kv in _fromServices)
            {
                raw[kv.Key] = TestMethodArguments[kv.Key];

                TestMethodArguments[kv.Key] = kv.Value == typeof(ITestOutputHelper)
                    ? testOutputHelper
                    : scope.ServiceProvider.GetService(kv.Value);
            }

            var item = await new DependencyInjectionTestInvoker(scope.ServiceProvider, Test, MessageBus, TestClass,
                        DependencyInjectionTestMethodRunner.CreateTestClassConstructorArguments(scope.ServiceProvider, ConstructorArguments, aggregator),
                        TestMethod, TestMethodArguments, BeforeAfterAttributes, aggregator, CancellationTokenSource)
                    .RunAsync().ConfigureAwait(false);

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
    }
}
