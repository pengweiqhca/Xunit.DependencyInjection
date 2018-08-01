using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestRunner : XunitTestMethodRunner
    {
        private readonly IServiceProvider _provider;

        public DependencyInjectionTestRunner(IServiceProvider provider, ITestMethod testMethod, IReflectionTypeInfo @class, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, object[] constructorArguments)
            : base(testMethod, @class, method, testCases, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource, constructorArguments)
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
