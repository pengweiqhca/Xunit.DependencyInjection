namespace Xunit.DependencyInjection;

public class ContextValue<T>
{
    private readonly AsyncLocal<ValueHolder> _value = new();

    public virtual T? Value
    {
        get
        {
            var value = _value.Value;

            return value == null ? default : value.Value;
        }
        set
        {
            var holder = _value.Value;
            if (holder != null)

                // Clear current value trapped in the AsyncLocals, as its done.
                holder.Value = default;

            if (value != null)

                // Use an object indirection to hold the value in the AsyncLocal,
                // so it can be cleared in all ExecutionContexts when its cleared.
                _value.Value = new() { Value = value };
        }
    }

    /// <summary>See <see href="https://github.com/dotnet/aspnetcore/blob/main/src/Http/Http/src/HttpContextAccessor.cs">HttpContextAccessor</see></summary>
    private class ValueHolder
    {
        public T? Value;
    }
}
