namespace Xunit.DependencyInjection.Test.Parallelization;

public class ParallelTheoryTests(ConcurrencyFixture fixture) : IClassFixture<ConcurrencyFixture>
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
    public async Task Theory(int _) => Assert.InRange(await fixture.CheckConcurrencyAsync(), 1, 2);
}
