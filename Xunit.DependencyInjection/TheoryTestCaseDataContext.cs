using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.DependencyInjection
{
    internal static class TheoryTestCaseDataContext
    {
        private static readonly AsyncLocal<IServiceProvider?> AsyncLocalServices = new();

        public static IServiceProvider? Services { get => AsyncLocalServices.Value; private set => AsyncLocalServices.Value = value; }

        public static IAsyncDisposable BeginContext(IServiceProvider provider) =>
            new Disposable(provider.CreateScope());

        private class Disposable : IAsyncDisposable
        {
            private readonly IServiceScope _scope;

            public Disposable(IServiceScope scope)
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
}
