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
    public class DependencyInjectionTestClassRunner : XunitTestClassRunner
    {
        private readonly IServiceProvider _provider;
        private IServiceScope? _serviceScope;

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

        /// <inheritdoc />
        protected override object?[] CreateTestClassConstructorArguments()
        {
            if ((!Class.Type.GetTypeInfo().IsAbstract ? 0 : (Class.Type.GetTypeInfo().IsSealed ? 1 : 0)) != 0)
                return new object?[0];

            var constructor = SelectTestClassConstructor();
            if (constructor == null)
                return new object?[0];

            var parameters = constructor.GetParameters();
            if (parameters.Length > 0)
                _provider.GetRequiredService<ITestOutputHelperAccessor>().Output = new TestOutputHelper();

            var objArray = new object?[parameters.Length];
            for (var index = 0; index < parameters.Length; ++index)
            {
                var parameterInfo = parameters[index];
                if (TryGetConstructorArgument(constructor, index, parameterInfo, out var argumentValue))
                    objArray[index] = argumentValue;
                else
                    objArray[index] = new DelayArgument(parameterInfo, unusedArguments => FormatConstructorArgsMissingMessage(constructor, unusedArguments));
            }

            return objArray;
        }

        /// <inheritdoc />
        protected override bool TryGetConstructorArgument(ConstructorInfo constructor, int index, ParameterInfo parameter, out object? argumentValue)
        {
            if (parameter.ParameterType == typeof(ITestOutputHelper))
            {
                argumentValue = _provider.GetRequiredService<ITestOutputHelperAccessor>().Output;
                return true;
            }

            if (parameter.ParameterType == typeof(CancellationToken))
            {
                argumentValue = CancellationTokenSource.Token;
                return true;
            }

            return base.TryGetConstructorArgument(constructor, index, parameter, out argumentValue);
        }

        /// <inheritdoc />
        protected override void CreateClassFixture(Type fixtureType)
        {
            _serviceScope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            Aggregator.Run(() => ClassFixtureMappings[fixtureType] = ActivatorUtilities.GetServiceOrCreateInstance(_serviceScope.ServiceProvider, fixtureType));
        }

        /// <inheritdoc />
        protected override async Task BeforeTestClassFinishedAsync()
        {
            await base.BeforeTestClassFinishedAsync();

            _serviceScope?.Dispose();
        }

        internal class DelayArgument
        {
            public DelayArgument(ParameterInfo parameter, Func<IReadOnlyList<Tuple<int, ParameterInfo>>, string> formatConstructorArgsMissingMessage)
            {
                FormatConstructorArgsMissingMessage = formatConstructorArgsMissingMessage;
                Parameter = parameter;
            }

            public ParameterInfo Parameter { get; }

            public Func<IReadOnlyList<Tuple<int, ParameterInfo>>, string> FormatConstructorArgsMissingMessage { get; }

            public bool TryGetConstructorArgument(IServiceProvider provider, ExceptionAggregator aggregator, out object? argumentValue)
            {
                argumentValue = null;

                try
                {
                    argumentValue = provider.GetService(Parameter.ParameterType);
                }
                catch (Exception ex)
                {
                    aggregator.Add(ex);

                    return true;
                }

                if (argumentValue != null)
                    return true;

                if (Parameter.HasDefaultValue)
                    argumentValue = Parameter.DefaultValue;
                else if (Parameter.IsOptional)
                    argumentValue = GetDefaultValue(Parameter.ParameterType);
                else if (Parameter.GetCustomAttribute<ParamArrayAttribute>() != null)
                    argumentValue = Array.CreateInstance(Parameter.ParameterType, new int[1]);
                else
                    return false;

                return true;
            }
        }

        private static object? GetDefaultValue(Type typeInfo) =>
            typeInfo.GetTypeInfo().IsValueType ? Activator.CreateInstance(typeInfo) : null;

        /// <inheritdoc />
        protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod,
            IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, object[] constructorArguments) =>
            new DependencyInjectionTestMethodRunner(_provider, testMethod, Class, method,
                    testCases, DiagnosticMessageSink, MessageBus, new ExceptionAggregator(Aggregator),
                    CancellationTokenSource, constructorArguments)
                .RunAsync();
    }
}
