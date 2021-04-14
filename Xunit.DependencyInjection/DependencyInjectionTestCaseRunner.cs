using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestCaseRunner : XunitTestCaseRunner
    {
        private readonly IServiceProvider _provider;

        public DependencyInjectionTestCaseRunner(IServiceProvider provider, IXunitTestCase testCase,
            string displayName, string skipReason, object?[] constructorArguments, object[] testMethodArguments,
            IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
            : base(testCase, displayName, skipReason, constructorArguments, testMethodArguments, messageBus,
                aggregator, cancellationTokenSource) => _provider = provider;

        /// <inheritdoc />
        protected override Task<RunSummary> RunTestAsync() =>
            new DependencyInjectionTestRunner(_provider, new XunitTest(TestCase, DisplayName), MessageBus,
                    FromServicesAttribute.CreateFromServices(TestMethod),
                    TestClass, ConstructorArguments, TestMethod, TestMethodArguments, SkipReason,
                    BeforeAfterAttributes, new ExceptionAggregator(Aggregator), CancellationTokenSource)
                .RunAsync();
    }

    public class DependencyInjectionTheoryTestCaseRunner : XunitTheoryTestCaseRunner
    {
        private static readonly Func<XunitTheoryTestCaseRunner, List<XunitTestRunner>> GetTestRunners;
        private static readonly Func<TestRunner<IXunitTestCase>, ITest> GetTest;
        private static readonly Func<TestRunner<IXunitTestCase>, object[]> GetTestMethodArguments;
        private readonly IServiceProvider _provider;

        static DependencyInjectionTheoryTestCaseRunner()
        {
            var testCaseRunner = Expression.Parameter(typeof(XunitTheoryTestCaseRunner));

            GetTestRunners = Expression.Lambda<Func<XunitTheoryTestCaseRunner, List<XunitTestRunner>>>(Expression.PropertyOrField(testCaseRunner, "testRunners"), testCaseRunner).Compile();

            var testRunner = Expression.Parameter(typeof(TestRunner<IXunitTestCase>));

            GetTest = Expression.Lambda<Func<TestRunner<IXunitTestCase>, ITest>>(Expression.PropertyOrField(testRunner, "Test"), testRunner).Compile();
            GetTestMethodArguments = Expression.Lambda<Func<TestRunner<IXunitTestCase>, object[]>>(Expression.PropertyOrField(testRunner, "TestMethodArguments"), testRunner).Compile();
        }

        public DependencyInjectionTheoryTestCaseRunner(IServiceProvider provider, IXunitTestCase testCase,
            string displayName, string skipReason, object?[] constructorArguments, IMessageSink diagnosticMessageSink,
            IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
            : base(testCase, displayName, skipReason, constructorArguments, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource) =>
            _provider = provider;

        /// <inheritdoc />
        protected override async Task AfterTestCaseStartingAsync()
        {
            await base.AfterTestCaseStartingAsync().ConfigureAwait(false);

            var fromServices = FromServicesAttribute.CreateFromServices(TestMethod);
            var runners = GetTestRunners(this);
            for (var index = 0; index < runners.Count; index++)
            {
                if (runners[index] is TestRunner<IXunitTestCase> runner)
                    runners[index] = new DependencyInjectionTestRunner(_provider, GetTest(runner),
                        MessageBus, fromServices, TestClass, ConstructorArguments, TestMethod, GetTestMethodArguments(runner),
                        SkipReason, BeforeAfterAttributes, Aggregator, CancellationTokenSource);
            }
        }
    }
}
