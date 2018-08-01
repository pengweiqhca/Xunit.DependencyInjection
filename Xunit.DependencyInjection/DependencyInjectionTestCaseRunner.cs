using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestCaseRunner : TestCaseRunner<XunitTestCase>
    {
        private readonly IServiceProvider _provider;
        private readonly object[] _constructorArguments;

        public DependencyInjectionTestCaseRunner(IServiceProvider provider, object[] constructorArguments, XunitTestCase testCase, 
            IMessageBus messageBus,
            ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource) : base(testCase,
            messageBus, aggregator, cancellationTokenSource)
        {
            _provider = provider;
            _constructorArguments = constructorArguments;

            if (testCase == null)
            {
                throw new ArgumentNullException(nameof(XunitTestCase));
            }
        }

        protected override Task<RunSummary> RunTestAsync()
        {
            var testClass = TestCase.TestMethod.TestClass.Class.ToRuntimeType();
            var testMethod = TestCase.TestMethod.Method.ToRuntimeMethod();
            var test = new XunitTest(TestCase, this.TestCase.DisplayName);

            return new DependencyInjectionTestRunner(_provider,
                    test,
                    MessageBus, 
                    testClass, 
                    _constructorArguments,
                    testMethod,
                    TestCase.TestMethodArguments, 
                    TestCase.SkipReason, 
                    Aggregator,
                    CancellationTokenSource)
                .RunAsync();
        }
    }
}
