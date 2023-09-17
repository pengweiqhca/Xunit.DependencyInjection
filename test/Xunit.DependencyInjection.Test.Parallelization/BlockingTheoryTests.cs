namespace Xunit.DependencyInjection.Test.Parallelization;

public class BlockingTheoryTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture _fixture;

    public BlockingTheoryTests(ConcurrencyFixture fixture) => this._fixture = fixture;

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Theory(int _) => Assert.Equal(2, await _fixture.CheckConcurrencyAsync().ConfigureAwait(false));
}
