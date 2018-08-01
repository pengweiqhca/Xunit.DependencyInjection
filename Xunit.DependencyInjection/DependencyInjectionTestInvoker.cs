using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestInvoker : TestInvoker<IXunitTestCase>
    {
        private readonly ITestOutputHelperAccessor _accessor;

        public DependencyInjectionTestInvoker(IServiceProvider provider, ITest test, IMessageBus messageBus, Type testClass,
            object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments,
            ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource) : base(test, messageBus,
            testClass, constructorArguments, testMethod, testMethodArguments, aggregator, cancellationTokenSource)
        {
            _accessor = provider.GetRequiredService<ITestOutputHelperAccessor>();
        }

        public string Output { get; set; }

        protected override Task BeforeTestMethodInvokedAsync()
        {
            if (_accessor.Output is TestOutputHelper output)
                output.Initialize(MessageBus, Test);

            return base.BeforeTestMethodInvokedAsync();
        }

        protected override Task AfterTestMethodInvokedAsync()
        {
            if (_accessor.Output is TestOutputHelper output)
            {
                Output = output.Output;
                output.Uninitialize();
            }

            return base.AfterTestMethodInvokedAsync();
        }
    }
}
