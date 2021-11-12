namespace Xunit.DependencyInjection;

internal static class DisposableExtensions
{
    public static ValueTask DisposeAsync(this IDisposable disposable)
    {
        switch (disposable)
        {
            case null:
                throw new ArgumentNullException(nameof(disposable));
            case IAsyncDisposable ad:
                return ad.DisposeAsync();
            default:
                disposable.Dispose();

                return default;
        }
    }

    public static IAsyncDisposable AsAsyncDisposable(this IDisposable disposable) =>
        disposable switch
        {
            null => throw new ArgumentNullException(nameof(disposable)),
            IAsyncDisposable ad => ad,
            _ => new DisposableWrapper(disposable)
        };

    private class DisposableWrapper : IDisposable, IAsyncDisposable
    {
        private readonly IDisposable _disposable;

        public DisposableWrapper(IDisposable disposable) => _disposable = disposable;

        public void Dispose() => _disposable.Dispose();

        public ValueTask DisposeAsync()
        {
            _disposable.Dispose();

            return default;
        }
    }
}