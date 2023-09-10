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
}
