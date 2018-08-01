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
        public DependencyInjectionTestInvoker(IServiceProvider provider, ITest test, IMessageBus messageBus, Type testClass,
            object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments,
            ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource) : base(test, messageBus,
            testClass, constructorArguments, testMethod, testMethodArguments, aggregator, cancellationTokenSource)
        {
            _testOutputHelper = (TestOutputHelper)provider.GetService(typeof(ITestOutputHelper));
        }

        public string Output { get; set; }

        protected override Task BeforeTestMethodInvokedAsync()
        {
            _testOutputHelper?.Initialize(MessageBus, Test);

            return base.BeforeTestMethodInvokedAsync();
        }

        protected override Task AfterTestMethodInvokedAsync()
        {
            if (_testOutputHelper != null)
            {
                Output = _testOutputHelper.Output;
                _testOutputHelper.Uninitialize();
            }
            
            return base.AfterTestMethodInvokedAsync();
        }

        private readonly TestOutputHelper _testOutputHelper;
    }
}
