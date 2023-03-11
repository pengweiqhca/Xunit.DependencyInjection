using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Internal;

/// <summary>
/// Helper for detecting whether a given type is FSharpAsync`1, and if so, supplying
/// an <see cref="Expression"/> for mapping instances of that type to a C# awaitable.
/// </summary>
/// <remarks>
/// The main design goal here is to avoid taking a compile-time dependency on
/// FSharp.Core.dll, because non-F# applications wouldn't use it. So all the references
/// to FSharp types have to be constructed dynamically at runtime.
/// </remarks>
internal static class ObjectMethodExecutorFSharpSupport
{
    private static readonly object FsharpValuesCacheLock = new();
    private static Assembly? _fsharpCoreAssembly;
    private static MethodInfo? _fsharpAsyncStartAsTaskGenericMethod;
    private static PropertyInfo? _fsharpOptionOfTaskCreationOptionsNoneProperty;
    private static PropertyInfo? _fsharpOptionOfCancellationTokenNoneProperty;

    public static bool TryBuildCoercerFromFSharpAsyncToAwaitable(Type possibleFSharpAsyncType,
        [NotNullWhen(true)] out Expression? coerceToAwaitableExpression,
        [NotNullWhen(true)] out Type? awaitableType)
    {
        var methodReturnGenericType = possibleFSharpAsyncType.IsGenericType
            ? possibleFSharpAsyncType.GetGenericTypeDefinition()
            : null;

        if (!IsFSharpAsyncOpenGenericType(methodReturnGenericType))
        {
            coerceToAwaitableExpression = null;
            awaitableType = null;
            return false;
        }

        var awaiterResultType = possibleFSharpAsyncType.GetGenericArguments().Single();
        awaitableType = typeof(Task<>).MakeGenericType(awaiterResultType);

        /*coerceToAwaitableExpression = (object fsharpAsync) => (object)FSharpAsync.StartAsTask<TResult>(
            (Microsoft.FSharp.Control.FSharpAsync<TResult>)fsharpAsync,
            FSharpOption<TaskCreationOptions>.None,
            FSharpOption<CancellationToken>.None);*/
        var startAsTaskClosedMethod = _fsharpAsyncStartAsTaskGenericMethod.MakeGenericMethod(awaiterResultType);

        var coerceToAwaitableParam = Expression.Parameter(typeof(object));
        coerceToAwaitableExpression = Expression.Lambda(Expression.Convert(Expression.Call(
                startAsTaskClosedMethod,
                Expression.Convert(coerceToAwaitableParam, possibleFSharpAsyncType),
                Expression.MakeMemberAccess(null, _fsharpOptionOfTaskCreationOptionsNoneProperty),
                Expression.MakeMemberAccess(null, _fsharpOptionOfCancellationTokenNoneProperty)),
            typeof(object)), coerceToAwaitableParam);

        return true;
    }

    [MemberNotNullWhen(true, nameof(_fsharpAsyncStartAsTaskGenericMethod), nameof(_fsharpCoreAssembly),
        nameof(_fsharpOptionOfTaskCreationOptionsNoneProperty), nameof(_fsharpOptionOfCancellationTokenNoneProperty))]
    private static bool IsFSharpAsyncOpenGenericType(Type? possibleFSharpAsyncGenericType)
    {
        if (possibleFSharpAsyncGenericType == null || !string.Equals(possibleFSharpAsyncGenericType.FullName,
                "Microsoft.FSharp.Control.FSharpAsync`1", StringComparison.Ordinal)) return false;

        lock (FsharpValuesCacheLock)
            return _fsharpCoreAssembly != null
                // Since we've already found the real FSharpAsync.Core assembly, we just have to check that the supplied FSharpAsync`1 type is the one from that assembly.
                ? possibleFSharpAsyncGenericType.Assembly == _fsharpCoreAssembly
                // We'll keep trying to find the FSharp types/values each time any type called FSharpAsync`1 is supplied.
                : TryPopulateFSharpValueCaches(possibleFSharpAsyncGenericType);
    }

    private static bool TryPopulateFSharpValueCaches(Type possibleFSharpAsyncGenericType)
    {
        var assembly = possibleFSharpAsyncGenericType.Assembly;
        var fsharpOptionType = assembly.GetType("Microsoft.FSharp.Core.FSharpOption`1");
        var fsharpAsyncType = assembly.GetType("Microsoft.FSharp.Control.FSharpAsync");

        if (fsharpOptionType == null || fsharpAsyncType == null) return false;

        // Get a reference to FSharpOption<TaskCreationOptions>.None
        var fsharpOptionOfTaskCreationOptionsType = fsharpOptionType.MakeGenericType(typeof(TaskCreationOptions));
        _fsharpOptionOfTaskCreationOptionsNoneProperty = fsharpOptionOfTaskCreationOptionsType.GetRuntimeProperty("None")!;

        // Get a reference to FSharpOption<CancellationToken>.None
        var fsharpOptionOfCancellationTokenType = fsharpOptionType.MakeGenericType(typeof(CancellationToken));
        _fsharpOptionOfCancellationTokenNoneProperty = fsharpOptionOfCancellationTokenType.GetRuntimeProperty("None")!;

        // Get a reference to FSharpAsync.StartAsTask<>
        var fsharpAsyncMethods = fsharpAsyncType.GetRuntimeMethods()
            .Where(m => m.Name.Equals("StartAsTask", StringComparison.Ordinal));
        foreach (var candidateMethodInfo in fsharpAsyncMethods)
        {
            var parameters = candidateMethodInfo.GetParameters();
            if (parameters.Length == 3 &&
                TypesHaveSameIdentity(parameters[0].ParameterType, possibleFSharpAsyncGenericType) &&
                parameters[1].ParameterType == fsharpOptionOfTaskCreationOptionsType &&
                parameters[2].ParameterType == fsharpOptionOfCancellationTokenType)
            {
                // This really does look like the correct method (and hence assembly).
                _fsharpAsyncStartAsTaskGenericMethod = candidateMethodInfo;
                _fsharpCoreAssembly = assembly;
                break;
            }
        }

        return _fsharpCoreAssembly != null;
    }

    private static bool TypesHaveSameIdentity(Type type1, Type type2) =>
        type1.Assembly == type2.Assembly &&
        string.Equals(type1.Namespace, type2.Namespace, StringComparison.Ordinal) &&
        string.Equals(type1.Name, type2.Name, StringComparison.Ordinal);
}
