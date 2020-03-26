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
    public class DependencyInjectionTestMethodRunner : TestMethodRunner<IXunitTestCase>
    {
        private readonly IServiceProvider _provider;
        private readonly IMessageSink _diagnosticMessageSink;
        private readonly object[] _constructorArguments;

        public DependencyInjectionTestMethodRunner(IServiceProvider provider,
            ITestMethod testMethod,
            IReflectionTypeInfo @class,
            IReflectionMethodInfo method,
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource,
            object[] constructorArguments)
            : base(testMethod, @class, method, testCases, messageBus, aggregator, cancellationTokenSource)
        {
            _provider = provider;
            _diagnosticMessageSink = diagnosticMessageSink;
            _constructorArguments = constructorArguments;
        }

        protected object?[] CreateTestClassConstructorArguments(IServiceProvider provider)
        {
            var unusedArguments = new List<Tuple<int, ParameterInfo>>();
            Func<IReadOnlyList<Tuple<int, ParameterInfo>>, string>? formatConstructorArgsMissingMessage = null;

            var args = new object?[_constructorArguments.Length];
            for (var index = 0; index < _constructorArguments.Length; index++)
            {
                if (_constructorArguments[index] is DependencyInjectionTestClassRunner.DelayArgument delay)
                {
                    formatConstructorArgsMissingMessage = delay.FormatConstructorArgsMissingMessage;

                    if (delay.TryGetConstructorArgument(provider, Aggregator, out var arg))
                        args[index] = arg;
                    else
                        unusedArguments.Add(Tuple.Create(index, delay.Parameter));
                }
                else
                    args[index] = _constructorArguments[index];
            }

            if (unusedArguments.Count > 0 && formatConstructorArgsMissingMessage != null)
                Aggregator.Add(new TestClassException(formatConstructorArgsMissingMessage(unusedArguments)));

            return args;
        }

        /// <inheritdoc />
        protected override async Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
        {
            if (testCase is ExecutionErrorTestCase)
                return await testCase.RunAsync(_diagnosticMessageSink, MessageBus,
                    _constructorArguments, new ExceptionAggregator(Aggregator), CancellationTokenSource);

            using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            XunitTestCaseRunner runner;
            if (testCase is XunitTheoryTestCase)
                runner = new DependencyInjectionTheoryTestCaseRunner(scope.ServiceProvider, testCase,
                    testCase.DisplayName, testCase.SkipReason,
                    CreateTestClassConstructorArguments(scope.ServiceProvider),
                    _diagnosticMessageSink, MessageBus, new ExceptionAggregator(Aggregator), CancellationTokenSource);
            else
                runner = new DependencyInjectionTestCaseRunner(scope.ServiceProvider, testCase,
                    testCase.DisplayName, testCase.SkipReason,
                    CreateTestClassConstructorArguments(scope.ServiceProvider), testCase.TestMethodArguments,
                    MessageBus, new ExceptionAggregator(Aggregator), CancellationTokenSource);

            return await runner.RunAsync();
        }
    }
}
