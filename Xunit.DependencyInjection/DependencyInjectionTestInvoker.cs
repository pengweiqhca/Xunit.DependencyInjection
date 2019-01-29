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
            Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments,
            IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
            : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments,
                beforeAfterAttributes, aggregator, cancellationTokenSource) =>
            _provider = provider;

        protected override object CallTestMethod(object testClassInstance)
        {
            var result = base.CallTestMethod(testClassInstance);

            return result is Task task ? AsyncStack(task) : result;

            async Task AsyncStack(Task t)
            {
                try
                {
                    await t;
                }
                catch (Exception ex)
                {
                    while (true)
                    {
                        if (ex is TargetInvocationException tie)
                            ex = tie.InnerException;
                        else
                            break;
                    }

                    Aggregator.Add(_provider.GetService<IAsyncExceptionFilter>()?.Process(ex) ?? ex);
                }
            }
        }
    }
}
