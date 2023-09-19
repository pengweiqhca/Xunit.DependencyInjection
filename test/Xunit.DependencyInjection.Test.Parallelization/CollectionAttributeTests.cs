namespace Xunit.DependencyInjection.Test.Parallelization;

[Collection("Sequential")]
public class CollectionAttributeTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture _fixture;

    public CollectionAttributeTests(ConcurrencyFixture fixture) => this._fixture = fixture;

    [Fact]
    public async Task Fact1() => Assert.Equal(1, await _fixture.CheckConcurrencyAsync().ConfigureAwait(false));

    [Fact]
    public async Task Fact2() => Assert.Equal(1, await _fixture.CheckConcurrencyAsync().ConfigureAwait(false));
}