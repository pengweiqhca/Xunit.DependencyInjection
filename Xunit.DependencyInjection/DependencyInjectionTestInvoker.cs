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
    public class DependencyInjectionTestInvoker : XunitTestInvoker
    {
        private readonly IServiceProvider _provider;

        public DependencyInjectionTestInvoker(IServiceProvider provider, ITest test, IMessageBus messageBus,
            Type testClass, object?[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments,
            IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
            : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments,
                beforeAfterAttributes, aggregator, cancellationTokenSource) =>
            _provider = provider;

        /// <inheritdoc />
        protected override object CallTestMethod(object testClassInstance)
        {
            var result = base.CallTestMethod(testClassInstance);

            return result is Task task ? AsyncStack(task) : result;

            async Task AsyncStack(Task t)
            {
                try
                {
                    await t.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    while (ex is TargetInvocationException { InnerException: { } } tie)
                    {
                        ex = tie.InnerException;
                    }

                    Aggregator.Add(_provider.GetService<IAsyncExceptionFilter>()?.Process(ex) ?? ex);
                }
            }
        }
    }
}
