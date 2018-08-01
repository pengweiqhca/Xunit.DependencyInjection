using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestRunner : TestRunner<IXunitTestCase>
    {
        private readonly IServiceProvider _provider;

        public DependencyInjectionTestRunner(IServiceProvider provider, ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, string skipReason, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource) : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, skipReason, aggregator, cancellationTokenSource)
        {
            _provider = provider;
        }

        protected override async Task<Tuple<decimal, string>> InvokeTestAsync(ExceptionAggregator aggregator)
        {
            var invoker = new DependencyInjectionTestInvoker(_provider, Test, MessageBus, TestClass, ConstructorArguments, TestMethod, TestMethodArguments, aggregator, CancellationTokenSource);
            var duration = await invoker.RunAsync();

            return Tuple.Create(duration, invoker.Output);
        }
    }
}
