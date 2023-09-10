namespace Xunit.DependencyInjection;

internal static class TheoryTestCaseDataContext
{
    private static readonly ContextValue<IServiceProvider?> AsyncLocalServices = new();

    public static IServiceProvider? Services { get => AsyncLocalServices.Value; private set => AsyncLocalServices.Value = value; }

    public static IAsyncDisposable BeginContext(IServiceProvider provider) =>
        new Disposable(provider.CreateAsyncScope());

    private class Disposable : IAsyncDisposable
    {
        private readonly AsyncServiceScope _scope;

        public Disposable(AsyncServiceScope scope)
        {
            Services = scope.ServiceProvider;

            _scope = scope;
        }

        public ValueTask DisposeAsync()
        {
            Services = null;

            return _scope.DisposeAsync();
        }
    }
}
