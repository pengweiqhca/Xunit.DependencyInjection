namespace Xunit.DependencyInjection.Test;

public class StaFactTest : IDisposable, IAsyncLifetime
{
    private readonly SynchronizationContext? _ctorSyncContext;
    private readonly int _ctorThreadId;

    public StaFactTest(IDependency _)
    {
        _ctorSyncContext = SynchronizationContext.Current;

        _ctorThreadId = Environment.CurrentManagedThreadId;

        Assert.NotNull(_ctorSyncContext);
    }

    public void Dispose()
    {
        Assert.Equal(_ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(_ctorSyncContext, SynchronizationContext.Current);
    }

    public async Task InitializeAsync()
    {
        Assert.Equal(_ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(_ctorSyncContext, SynchronizationContext.Current);
        await Task.Yield();
        Assert.Equal(_ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(_ctorSyncContext, SynchronizationContext.Current);
    }

    public async Task DisposeAsync()
    {
        Assert.Equal(_ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(_ctorSyncContext, SynchronizationContext.Current);
        await Task.Yield();
        Assert.Equal(_ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(_ctorSyncContext, SynchronizationContext.Current);
    }

    [UIFact]
    public void CtorAndTestMethodInvokedInSameContext()
    {
        Assert.Equal(_ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(_ctorSyncContext, SynchronizationContext.Current);
    }

    [UITheory]
    [InlineData(0)]
    public async Task CtorAndTestMethodInvokedInSameContext_AcrossYields(int arg)
    {
        await Task.Yield();
        Assert.Equal(_ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(_ctorSyncContext, SynchronizationContext.Current);
        Assert.Equal(0, arg);
    }
}
