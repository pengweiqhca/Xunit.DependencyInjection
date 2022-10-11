using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Internal;

internal readonly struct CoercedAwaitableInfo
{
    public AwaitableInfo AwaitableInfo { get; }
    public Expression? CoercerExpression { get; }
    public Type? CoercerResultType { get; }

    [MemberNotNullWhen(true, nameof(CoercerExpression))]
    public bool RequiresCoercion => CoercerExpression != null;

    public CoercedAwaitableInfo(AwaitableInfo awaitableInfo)
    {
        AwaitableInfo = awaitableInfo;
        CoercerExpression = null;
        CoercerResultType = null;
    }

    public CoercedAwaitableInfo(Expression coercerExpression, Type coercerResultType, AwaitableInfo coercedAwaitableInfo)
    {
        CoercerExpression = coercerExpression;
        CoercerResultType = coercerResultType;
        AwaitableInfo = coercedAwaitableInfo;
    }

    public static bool IsTypeAwaitable(Type type, out CoercedAwaitableInfo info)
    {
        if (AwaitableInfo.IsTypeAwaitable(type, out var directlyAwaitableInfo))
        {
            info = new(directlyAwaitableInfo);
            return true;
        }

        // It's not directly awaitable, but maybe we can coerce it.
        // Currently we support coercing FSharpAsync<T>.
        if (ObjectMethodExecutorFSharpSupport.TryBuildCoercerFromFSharpAsyncToAwaitable(type,
                out var coercerExpression, out var coercerResultType) &&
            AwaitableInfo.IsTypeAwaitable(coercerResultType, out var coercedAwaitableInfo))
        {
            info = new(coercerExpression, coercerResultType, coercedAwaitableInfo);
            return true;
        }

        info = default;
        return false;
    }
}
