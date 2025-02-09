namespace Xunit.DependencyInjection.Test.Parallelization;

[DisableParallelization]
public class DisabledParallelizationSequentialTheoryTests(ConcurrencyDisableFixture fixture)
    : IClassFixture<ConcurrencyDisableFixture>
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
    public Task Theory(int _) => fixture.CheckConcurrencyAsync();
}
