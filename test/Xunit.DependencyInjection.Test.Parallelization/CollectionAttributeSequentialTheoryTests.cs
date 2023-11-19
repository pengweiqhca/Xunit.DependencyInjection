namespace Xunit.DependencyInjection.Test.Parallelization;

[Collection("CollectionAttribute_SequentialTheoryTests")]
public class CollectionAttributeSequentialTheoryTests(ConcurrencyFixture fixture) : IClassFixture<ConcurrencyFixture>
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Theory(int _) => Assert.Equal(1, await fixture.CheckConcurrencyAsync());
}
