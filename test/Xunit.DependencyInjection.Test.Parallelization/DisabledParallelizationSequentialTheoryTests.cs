namespace Xunit.DependencyInjection.Test.Parallelization;

[DisableParallelization]
public class DisabledParallelizationSequentialTheoryTests(ConcurrencyFixture fixture)
    : IClassFixture<ConcurrencyFixture>
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Theory(int _) => Assert.Equal(1, await fixture.CheckConcurrencyAsync());
}
