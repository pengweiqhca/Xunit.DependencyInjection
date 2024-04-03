namespace Xunit.DependencyInjection;

internal static class TestHelper
{
    public static object?[] CreateTestClassConstructorArguments(this IServiceProvider provider,
        object?[] constructorArguments, ExceptionAggregator aggregator)
    {
        var unusedArguments = new List<Tuple<int, ParameterInfo>>();
        Func<IReadOnlyList<Tuple<int, ParameterInfo>>, string>? formatConstructorArgsMissingMessage = null;

        var args = new object?[constructorArguments.Length];
        for (var index = 0; index < constructorArguments.Length; index++)
            if (constructorArguments[index] is DelayArgument delay)
            {
                formatConstructorArgsMissingMessage = delay.FormatConstructorArgsMissingMessage;

                if (delay.TryGetConstructorArgument(provider, aggregator, out var arg))
                    args[index] = arg;
                else
                    unusedArguments.Add(Tuple.Create(index, delay.Parameter));
            }
            else
                args[index] = constructorArguments[index] is TestOutputHelperArgument
                    ? provider.GetRequiredService<ITestOutputHelperAccessor>().Output
                    : constructorArguments[index];

        if (unusedArguments.Count > 0 && formatConstructorArgsMissingMessage != null)
            aggregator.Add(new TestClassException(formatConstructorArgsMissingMessage(unusedArguments)));

        return args;
    }

    public class DelayArgument(
        ParameterInfo parameter,
        Func<IReadOnlyList<Tuple<int, ParameterInfo>>, string> formatConstructorArgsMissingMessage)
    {
        public ParameterInfo Parameter { get; } = parameter;

        public Func<IReadOnlyList<Tuple<int, ParameterInfo>>, string> FormatConstructorArgsMissingMessage { get; } = formatConstructorArgsMissingMessage;

        public bool TryGetConstructorArgument(IServiceProvider provider, ExceptionAggregator aggregator,
            out object? argumentValue)
        {
            argumentValue = null;

            try
            {
                argumentValue = provider.GetService(Parameter);
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

        private static object? GetDefaultValue(Type typeInfo) =>
            typeInfo.GetTypeInfo().IsValueType ? Activator.CreateInstance(typeInfo) : null;
    }

    public class TestOutputHelperArgument
    {
        private TestOutputHelperArgument() { }

        public static TestOutputHelperArgument Instance { get; } = new();
    }

    public static Exception Unwrap(this Exception ex)
    {
        while (ex is TargetInvocationException { InnerException: not null } tie) ex = tie.InnerException!;

        while (ex is AggregateException { InnerException: not null } ae) ex = ae.InnerException!;

        return ex;
    }
}
