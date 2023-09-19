namespace Xunit.DependencyInjection.Test.Parallelization;

public class ParallelTheoryTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture _fixture;

    public ParallelTheoryTests(ConcurrencyFixture fixture) => this._fixture = fixture;

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Theory(int _) => Assert.Equal(2, await _fixture.CheckConcurrencyAsync());
}
