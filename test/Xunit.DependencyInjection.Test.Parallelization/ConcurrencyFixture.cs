namespace Xunit.DependencyInjection.Test.Parallelization;

public class ConcurrencyFixture
{
    private int _concurrency;

    public async Task<int> CheckConcurrencyAsync()
    {
        Interlocked.Increment(ref _concurrency);

        await Delay(500);

        var overlap = _concurrency;

        await Delay(500);

        Interlocked.Decrement(ref _concurrency);

        return overlap;

        static ValueTask Delay(int millisecondsDelay)
        {
            // xunit 2.8.0+ and `parallelAlgorithm` is not `aggressive`
            if (SynchronizationContext.Current == null)
                return new(Task.Delay(millisecondsDelay));

            // xunit lt 2.8.0+ or `parallelAlgorithm` is `aggressive`
            // MaxConcurrencySyncContext is limit thread rather than task, so we should use `Thread.Sleep` rather than `Task.Delay`.
            Thread.Sleep(millisecondsDelay);

            return default;
        }
    }
}
