namespace Xunit.DependencyInjection.Test.Parallelization;

[DisableParallelization]
public class DisabledParallelizationSequentialTheoryTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture _fixture;

    public DisabledParallelizationSequentialTheoryTests(ConcurrencyFixture fixture) => this._fixture = fixture;

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Theory(int _) => Assert.Equal(1, await _fixture.CheckConcurrencyAsync().ConfigureAwait(false));
}
