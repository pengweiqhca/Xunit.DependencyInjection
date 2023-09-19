namespace Xunit.DependencyInjection.Test.Parallelization;

public class SequentialTheoryTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture _fixture;

    public SequentialTheoryTests(ConcurrencyFixture fixture) => this._fixture = fixture;

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [DisableParallelization]
    public async Task Theory(int _) => Assert.Equal(1, await _fixture.CheckConcurrencyAsync());
}
