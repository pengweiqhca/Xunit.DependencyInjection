namespace Xunit.DependencyInjection.Test.Parallelization;

[DisableParallelization]
public class SequentialCollectionTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture _fixture;

    public SequentialCollectionTests(ConcurrencyFixture fixture) => this._fixture = fixture;

    [Fact]
    public async Task Fact1() => Assert.Equal(1, await _fixture.CheckConcurrencyAsync());

    [Fact]
    public async Task Fact2() => Assert.Equal(1, await _fixture.CheckConcurrencyAsync());
}
