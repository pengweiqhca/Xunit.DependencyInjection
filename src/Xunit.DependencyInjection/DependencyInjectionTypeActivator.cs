using System.Globalization;
using Xunit.Internal;

namespace Xunit.DependencyInjection;

public class DependencyInjectionTypeActivator : ITypeActivator
{
    private readonly ContextValue<IServiceProvider?> _servicesContext = new();

    public IServiceProvider Services
    {
        get => _servicesContext.Value ?? throw new InvalidOperationException("Not set service provider.");
        set => _servicesContext.Value = value;
    }

    public static DependencyInjectionTypeActivator Instance { get; } = new();

    private DependencyInjectionTypeActivator() { }

    object ITypeActivator.CreateInstance(ConstructorInfo constructor,
        object?[]? arguments, Func<Type, IReadOnlyCollection<ParameterInfo>, string> missingArgumentMessageFormatter)
    {
        Guard.ArgumentNotNull(constructor);
        Guard.ArgumentNotNull(missingArgumentMessageFormatter);

        var type = constructor.ReflectedType
            ?? constructor.DeclaringType
            ?? throw new ArgumentException("Untyped constructors are not permitted", nameof(constructor));

        if (arguments is not null)
        {
            var parameters = constructor.GetParameters();
            if (parameters.Length != arguments.Length)
                throw new TestPipelineException(string.Format(
                    CultureInfo.CurrentCulture,
                    "Cannot create type '{0}' due to parameter count mismatch (needed {1}, got {2})",
                    type.SafeName(),
                    parameters.Length,
                    arguments.Length));

            for (var index = 0; index < parameters.Length; index++)
            {
                switch (arguments[index])
                {
                    case Missing:
                    case Array { Length: 0 }:
                        if (Services.GetService(parameters[index]) is { } service)
                            arguments[index] = service;

                        break;
                    case TestHelper.TestOutputHelperArgument:
                        arguments[index] = Services.GetRequiredService<ITestOutputHelperAccessor>().Output;
                        break;
                }
            }

            var missingArguments = arguments
                .Select((a, idx) => a is Missing ? parameters[idx] : null)
                .WhereNotNull()
                .CastOrToReadOnlyCollection();

            if (missingArguments.Count != 0)
                throw new TestPipelineException(missingArgumentMessageFormatter(type, missingArguments));
        }

        return constructor.Invoke(arguments);
    }
}
