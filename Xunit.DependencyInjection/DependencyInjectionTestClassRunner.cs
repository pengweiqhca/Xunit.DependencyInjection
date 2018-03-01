using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestClassRunner : XunitTestClassRunner
    {
        private readonly IServiceProvider _provider;

        public DependencyInjectionTestClassRunner(IServiceProvider provider,
            ITestClass testClass,
            IReflectionTypeInfo @class,
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            ITestCaseOrderer testCaseOrderer,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource,
            IDictionary<Type, object> collectionFixtureMappings)
            : base(testClass, @class, testCases, diagnosticMessageSink,
                messageBus, testCaseOrderer, aggregator,
                cancellationTokenSource, collectionFixtureMappings) =>
            _provider = provider;
        
        protected override bool TryGetConstructorArgument(ConstructorInfo constructor, int index,
            ParameterInfo parameter, out object argumentValue)
        {
            if (base.TryGetConstructorArgument(constructor, index, parameter, out argumentValue))
                return true;

            argumentValue = _provider.GetService(parameter.ParameterType);

            return argumentValue != null;
        }
    }
}
