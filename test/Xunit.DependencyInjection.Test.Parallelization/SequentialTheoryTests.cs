namespace Xunit.DependencyInjection.Test.Parallelization;

public class SequentialTheoryTests(ConcurrencyDisableFixture fixture) : IClassFixture<ConcurrencyDisableFixture>
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [DisableParallelization]
    public Task Theory(int _) => fixture.CheckConcurrencyAsync();
}
