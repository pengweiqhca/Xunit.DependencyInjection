namespace Xunit.DependencyInjection.Test.Parallelization;

[Collection("CollectionAttribute_SequentialTheoryTests")]
public class CollectionAttributeSequentialTheoryTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture _fixture;

    public CollectionAttributeSequentialTheoryTests(ConcurrencyFixture fixture) => this._fixture = fixture;

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Theory(int _) => Assert.Equal(1, await _fixture.CheckConcurrencyAsync().ConfigureAwait(false));
}
