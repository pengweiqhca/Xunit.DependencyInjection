using System.Collections.Concurrent;
using Xunit.Sdk;

namespace Xunit.DependencyInjection.Test.Parallelization;

public class ConcurrencyFixture
{
    private readonly bool _enableParallelization;
    private readonly ConcurrentBag<int> _results = new();
    private int _concurrency;

    public ConcurrencyFixture() => _enableParallelization = true;

    protected ConcurrencyFixture(bool enableParallelization) => _enableParallelization = enableParallelization;

    public async Task CheckConcurrencyAsync()
    {
        Interlocked.Increment(ref _concurrency);

        await Delay(500);

        var overlap = _concurrency;

        await Delay(500);

        Interlocked.Decrement(ref _concurrency);

        _results.Add(overlap);

        if (_enableParallelization) Assert.InRange(overlap, 1, 2);
        else Assert.Equal(1, overlap);

        CheckConcurrency(overlap);

        static ValueTask Delay(int millisecondsDelay)
        {
            // xunit 2.8.0+ and `parallelAlgorithm` is not `aggressive`
            if (SynchronizationContext.Current is null or not AsyncTestSyncContext)
                return new(Task.Delay(millisecondsDelay));

            // xunit lt 2.8.0+ or `parallelAlgorithm` is `aggressive`
            // MaxConcurrencySyncContext is limit thread rather than task, so we should use `Thread.Sleep` rather than `Task.Delay`.
            Thread.Sleep(millisecondsDelay);

            return default;
        }
    }

    private void CheckConcurrency(int overlap)
    {
        var dictionary = _results.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

        if (_enableParallelization) Assert.InRange(dictionary.Count, 1, 2);
        else Assert.Single(dictionary);

        Assert.Contains(overlap, dictionary);
    }
}

public class ConcurrencyDisableFixture() : ConcurrencyFixture(false);
