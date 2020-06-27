namespace Xunit.DependencyInjection.Test
{
    public interface IDependencyWithManagedLifetime
    {
         bool IsDisposed { get; }
    }
}
