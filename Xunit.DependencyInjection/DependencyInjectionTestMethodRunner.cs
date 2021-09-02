using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
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

        protected internal static object?[] CreateTestClassConstructorArguments(IServiceProvider provider,
            object[] constructorArguments, ExceptionAggregator aggregator)
        {
            var unusedArguments = new List<Tuple<int, ParameterInfo>>();
            Func<IReadOnlyList<Tuple<int, ParameterInfo>>, string>? formatConstructorArgsMissingMessage = null;

            var args = new object?[constructorArguments.Length];
            for (var index = 0; index < constructorArguments.Length; index++)
            {
                if (constructorArguments[index] is DependencyInjectionTestClassRunner.DelayArgument delay)
                {
                    formatConstructorArgsMissingMessage = delay.FormatConstructorArgsMissingMessage;

                    if (delay.TryGetConstructorArgument(provider, aggregator, out var arg))
                        args[index] = arg;
                    else
                        unusedArguments.Add(Tuple.Create(index, delay.Parameter));
                }
                else
                    args[index] = constructorArguments[index];
            }

            if (unusedArguments.Count > 0 && formatConstructorArgsMissingMessage != null)
                aggregator.Add(new TestClassException(formatConstructorArgsMissingMessage(unusedArguments)));

            return args;
        }

        /// <inheritdoc />
        protected override async Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
        {
            if (testCase is ExecutionErrorTestCase)
                return await testCase.RunAsync(_diagnosticMessageSink, MessageBus, _constructorArguments,
                        new ExceptionAggregator(Aggregator), CancellationTokenSource)
                    .ConfigureAwait(false);

            var wrappers = _provider.GetServices<IXunitTestCaseRunnerWrapper>().Reverse().ToArray();

            var type = testCase.GetType();
            do
            {
                var adapter = wrappers.FirstOrDefault(w => w.TestCaseType == type);
                if (adapter != null)
                    return await adapter.RunAsync(testCase, _provider, _diagnosticMessageSink, MessageBus,
                            _constructorArguments, new ExceptionAggregator(Aggregator), CancellationTokenSource)
                        .ConfigureAwait(false);
            } while ((type = type.BaseType) != null);

            using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            return await testCase.RunAsync(_diagnosticMessageSink, MessageBus,
                    CreateTestClassConstructorArguments(scope.ServiceProvider, _constructorArguments, Aggregator),
                    new ExceptionAggregator(Aggregator), CancellationTokenSource)
                .ConfigureAwait(false);
        }
    }
}
