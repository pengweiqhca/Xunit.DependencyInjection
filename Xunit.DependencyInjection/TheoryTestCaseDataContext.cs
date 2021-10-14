using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace Xunit.DependencyInjection
{
    internal static class TheoryTestCaseDataContext
    {
        private static readonly AsyncLocal<IServiceProvider?> AsyncLocalServices = new();

        public static IServiceProvider? Services { get => AsyncLocalServices.Value; private set => AsyncLocalServices.Value = value; }

        public static IDisposable BeginContext(IServiceProvider provider) =>
            new Disposable(provider.CreateScope());

        private class Disposable : IDisposable
        {
            private readonly IServiceScope _scope;

            public Disposable(IServiceScope scope)
            {
                Services = scope.ServiceProvider;

                _scope = scope;
            }

            public void Dispose()
            {
                Services = null;

                _scope.Dispose();
            }
        }
    }
}
