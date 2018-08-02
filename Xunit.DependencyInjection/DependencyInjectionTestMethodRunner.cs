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
        private readonly object[] _constructorArguments;

        public DependencyInjectionTestMethodRunner(IServiceProvider provider,
            ITestMethod testMethod,
            IReflectionTypeInfo @class,
            IReflectionMethodInfo method,
            IEnumerable<IXunitTestCase> testCases,
            IMessageBus messageBus,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource,
            object[] constructorArguments)
            : base(testMethod, @class, method, testCases, messageBus, aggregator, cancellationTokenSource)
        {
            _provider = provider;
            _constructorArguments = constructorArguments;
        }

        protected object[] CreateTestClassConstructorArguments(IServiceProvider provider)
        {
            var unusedArguments = new List<Tuple<int, ParameterInfo>>();
            Func<IReadOnlyList<Tuple<int, ParameterInfo>>, string> formatConstructorArgsMissingMessage = null;

            var args = new object[_constructorArguments.Length];
            for (var index = 0; index < _constructorArguments.Length; index++)
            {
                if (_constructorArguments[index] is DependencyInjectionTestClassRunner.DelayArgument delay)
                {
                    formatConstructorArgsMissingMessage = delay.FormatConstructorArgsMissingMessage;

                    if (delay.TryGetConstructorArgument(provider, out var arg))
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

        protected override async Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
        {
            using (var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                return await new DependencyInjectionTestCaseRunner(scope.ServiceProvider, testCase,
                        testCase.DisplayName, testCase.SkipReason,
                        CreateTestClassConstructorArguments(scope.ServiceProvider), testCase.TestMethodArguments,
                        MessageBus, new ExceptionAggregator(Aggregator), CancellationTokenSource)
                    .RunAsync();
        }
    }
}
