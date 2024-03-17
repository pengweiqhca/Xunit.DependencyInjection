using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Internal;

internal readonly struct AwaitableInfo(
    Type awaiterType,
    PropertyInfo awaiterIsCompletedProperty,
    MethodInfo awaiterGetResultMethod,
    MethodInfo awaiterOnCompletedMethod,
    MethodInfo? awaiterUnsafeOnCompletedMethod,
    MethodInfo getAwaiterMethod)
{
    private const BindingFlags Everything = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
    private static readonly MethodInfo OnCompleted = typeof(INotifyCompletion).GetMethod(nameof(INotifyCompletion.OnCompleted), Everything, null, [typeof(Action)], null)!;
    private static readonly MethodInfo UnsafeOnCompleted = typeof(ICriticalNotifyCompletion).GetMethod(nameof(ICriticalNotifyCompletion.UnsafeOnCompleted), Everything, null, [typeof(Action)], null)!;

    public Type AwaiterType { get; } = awaiterType;

    public PropertyInfo AwaiterIsCompletedProperty { get; } = awaiterIsCompletedProperty;

    public MethodInfo AwaiterGetResultMethod { get; } = awaiterGetResultMethod;

    public MethodInfo AwaiterOnCompletedMethod { get; } = awaiterOnCompletedMethod;

    public MethodInfo? AwaiterUnsafeOnCompletedMethod { get; } = awaiterUnsafeOnCompletedMethod;

    public MethodInfo GetAwaiterMethod { get; } = getAwaiterMethod;

    public static bool IsTypeAwaitable(Type type, out AwaitableInfo awaitableInfo)
    {
        // Based on Roslyn code: http://source.roslyn.io/#Microsoft.CodeAnalysis.Workspaces/Shared/Extensions/ISymbolExtensions.cs,db4d48ba694b9347

        // Awaitable must have method matching "object GetAwaiter()"
        var getAwaiterMethod = type.GetMethod(nameof(Task.GetAwaiter), Everything, null, Type.EmptyTypes, null);
        if (getAwaiterMethod is null)
        {
            awaitableInfo = default;
            return false;
        }

        var awaiterType = getAwaiterMethod.ReturnType;

        // Awaiter must have property matching "bool IsCompleted { get; }"
        var isCompletedProperty = awaiterType.GetProperty(nameof(TaskAwaiter.IsCompleted), Everything, null, typeof(bool), Type.EmptyTypes, null);
        if (isCompletedProperty?.GetMethod is null)
        {
            awaitableInfo = default;
            return false;
        }

        // Awaiter must implement INotifyCompletion
        var implementsINotifyCompletion = typeof(INotifyCompletion).IsAssignableFrom(awaiterType);
        if (!implementsINotifyCompletion)
        {
            awaitableInfo = default;
            return false;
        }

        // INotifyCompletion supplies a method matching "void OnCompleted(Action action)"
        var onCompletedMethod = OnCompleted;

        // Awaiter optionally implements ICriticalNotifyCompletion
        var implementsICriticalNotifyCompletion = typeof(ICriticalNotifyCompletion).IsAssignableFrom(awaiterType);
        MethodInfo? unsafeOnCompletedMethod = null;
        if (implementsICriticalNotifyCompletion)
            // ICriticalNotifyCompletion supplies a method matching "void UnsafeOnCompleted(Action action)"
            unsafeOnCompletedMethod = UnsafeOnCompleted;

        // Awaiter must have method matching "void GetResult" or "T GetResult()"
        var getResultMethod = awaiterType.GetMethod(nameof(TaskAwaiter.GetResult), Everything, null, Type.EmptyTypes, null);
        if (getResultMethod is null)
        {
            awaitableInfo = default;
            return false;
        }

        awaitableInfo = new(
            awaiterType,
            isCompletedProperty,
            getResultMethod,
            onCompletedMethod,
            unsafeOnCompletedMethod,
            getAwaiterMethod);
        return true;
    }
}
