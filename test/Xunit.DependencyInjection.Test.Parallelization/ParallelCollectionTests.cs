namespace Xunit.DependencyInjection.Test.Parallelization;

public class ParallelCollectionTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture _fixture;

    public ParallelCollectionTests(ConcurrencyFixture fixture) => this._fixture = fixture;

    [Fact]
    public async Task Fact1() => Assert.Equal(2, await _fixture.CheckConcurrencyAsync());

    [Fact]
    public async Task Fact2() => Assert.Equal(2, await _fixture.CheckConcurrencyAsync());
}
