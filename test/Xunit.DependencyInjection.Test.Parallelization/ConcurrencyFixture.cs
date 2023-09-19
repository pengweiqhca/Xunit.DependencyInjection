namespace Xunit.DependencyInjection.Test.Parallelization;

public class ConcurrencyFixture
{
    private int _concurrency;

    public async Task<int> CheckConcurrencyAsync()
    {
        Interlocked.Increment(ref _concurrency);
        await Task.Delay(TimeSpan.FromMilliseconds(1000));

        var overlap = _concurrency;

        await Task.Delay(TimeSpan.FromMilliseconds(1000));
        Interlocked.Decrement(ref _concurrency);

        return overlap;
    }
}
